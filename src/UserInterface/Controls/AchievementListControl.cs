using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public abstract class AchievementListControl<T, TEntry> : FlowPanel, IAchievementControl
        where T : AchievementTableEntryDescription
    {
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly ContentsManager contentsManager;
        private readonly AchievementTableEntry achievement;
        private readonly T description;
        private readonly CollectionAchievementTable achievementDetails;
        private readonly List<WindowBase2> itemWindows = new List<WindowBase2>();
        
        protected IAchievementService AchievementService { get; }

        public AchievementListControl(
            IItemDetailWindowFactory itemDetailWindowFactory,
            IAchievementService achievementService,
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            T description)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.AchievementService = achievementService;
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.description = description;

            this.achievementDetails = this.AchievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.ControlPadding = new Vector2(7f);
        }

        public void BuildControl()
        {
            if (!string.IsNullOrEmpty(this.description.GameText))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = this.description.GameText,
                    AutoSizeHeight = true,
                    Width = this.ContentRegion.Width,
                    WrapText = true,
                };
            }

            if (!string.IsNullOrEmpty(this.description.GameHint))
            {
                _ = new Label()
                {
                    Parent = this,
                    Text = this.description.GameHint,
                    TextColor = Microsoft.Xna.Framework.Color.LightGray,
                    Width = this.ContentRegion.Width,
                    AutoSizeHeight = true,
                    WrapText = true,
                };
            }

            var panel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.LeftToRight,
                Width = this.ContentRegion.Width,
                ControlPadding = new Vector2(7f),
                HeightSizingMode = SizingMode.AutoSize,
            };

            _ = Task.Run(() =>
            {
                var finishedAchievement = this.AchievementService.HasFinishedAchievement(this.achievement.Id);
                var entries = this.GetEntries(this.description).ToArray();
                for (var i = 0; i < entries.Length; i++)
                {
                    var imagePanel = new Panel()
                    {
                        Parent = panel,
                        BackgroundTexture = this.contentsManager.GetTexture("collection_item_background.png"),
                        Width = 71,
                        Height = 71,
                    };

                    var control = this.CreateEntryControl(i, entries[i], imagePanel);

                    control.Location = new Point((imagePanel.Width - control.Width) / 2, (imagePanel.Height - control.Height) / 2);

                    this.ColorControl(control, finishedAchievement || this.AchievementService.HasFinishedAchievementBit(this.achievement.Id, i));

                    var index = i;
                    control.Click += (s, eventArgs) =>
                    {
                        var itemWindow = this.itemDetailWindowFactory.Create(this.GetDisplayName(entries[index]), this.achievementDetails.ColumnNames, this.achievementDetails.Entries[index], this.achievementDetails.Link);
                        itemWindow.Parent = GameService.Graphics.SpriteScreen;
                        itemWindow.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));
                        itemWindow.ToggleWindow();
                        this.itemWindows.Add(itemWindow);
                    };
                }
            });
        }

        protected abstract IEnumerable<TEntry> GetEntries(T description);

        protected abstract Control CreateEntryControl(int index, TEntry entry, Container parent);

        protected abstract void ColorControl(Control control, bool achievementBitFinished);

        protected abstract string GetDisplayName(TEntry entry);

        protected override void DisposeControl()
        {
            foreach (var item in this.itemWindows)
            {
                item.Dispose();
            }

            this.itemWindows.Clear();

            base.DisposeControl();
        }
    }
}
