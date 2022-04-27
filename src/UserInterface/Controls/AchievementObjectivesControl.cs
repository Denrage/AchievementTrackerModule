using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementObjectivesControl : FlowPanel, IAchievementControl
    {
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly IAchievementService achievementService;
        private readonly ContentsManager contentsManager;
        private readonly ObjectivesDescription description;
        private readonly CollectionAchievementTable achievementDetails;

        public AchievementObjectivesControl(
            IItemDetailWindowFactory itemDetailWindowFactory,
            IAchievementService achievementService,
            ContentsManager contentsManager,
            Achievement achievement,
            ObjectivesDescription description)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.achievementService = achievementService;
            this.contentsManager = contentsManager;
            this.description = description;
            this.achievementDetails = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);
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
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(7f),
            };

            _ = Task.Run(() =>
            {
                for (var i = 0; i < this.description.EntryList.Count; i++)
                {
                    var imagePanel = new Panel()
                    {
                        Parent = panel,
                        BackgroundTexture = this.contentsManager.GetTexture("collection_item_background.png"),
                        Width = 71,
                        Height = 71,
                    };

                    var label = new Label()
                    {
                        Parent = imagePanel,
                        Width = 64,
                        Height = 64,
                        Text = (i + 1).ToString(),
                        Font = Content.DefaultFont18,
                        VerticalAlignment = VerticalAlignment.Middle,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        ZIndex = 1,
                    };

                    var index = i;
                    label.Click += (s, eventArgs) =>
                    {
                        var itemWindow = this.itemDetailWindowFactory.Create(this.description.EntryList[index].DisplayName, this.achievementDetails.ColumnNames, this.achievementDetails.Entries[index], this.achievementDetails.Link);
                        itemWindow.Parent = GameService.Graphics.SpriteScreen;
                        itemWindow.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));
                        itemWindow.ToggleWindow();
                    };
                }
            });
        }
    }
}
