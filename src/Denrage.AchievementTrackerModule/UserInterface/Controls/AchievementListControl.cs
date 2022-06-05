using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public abstract class AchievementListControl<T, TEntry> : FlowPanel, IAchievementControl
        where T : AchievementTableEntryDescription
    {
        private readonly IItemDetailWindowManager itemDetailWindowManager;
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;
        private readonly ContentsManager contentsManager;
        private readonly AchievementTableEntry achievement;
        private readonly T description;
        private readonly CollectionAchievementTable achievementDetails;
        private readonly List<Control> itemControls = new List<Control>();
        private FormattedLabel.FormattedLabel gameTextLabel;
        private FormattedLabel.FormattedLabel gameHintLabel;
        private FlowPanel panel;

        protected IAchievementService AchievementService { get; }

        public AchievementListControl(
            IItemDetailWindowManager itemDetailWindowManager,
            IAchievementService achievementService,
            IFormattedLabelHtmlService formattedLabelHtmlService,
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            T description)
        {
            this.itemDetailWindowManager = itemDetailWindowManager;
            this.AchievementService = achievementService;
            this.formattedLabelHtmlService = formattedLabelHtmlService;
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.description = description;

            this.achievementDetails = this.AchievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id);
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.ControlPadding = new Vector2(7f);

            this.AchievementService.PlayerAchievementsLoaded += this.AchievementService_PlayerAchievementsLoaded;
        }

        private void AchievementService_PlayerAchievementsLoaded()
        {
            var finishedAchievement = this.AchievementService.HasFinishedAchievement(this.achievement.Id);
            for (var i = 0; i < this.itemControls.Count; i++)
            {
                this.ColorControl(this.itemControls[i], finishedAchievement || this.AchievementService.HasFinishedAchievementBit(this.achievement.Id, i));
            }
        }

        public void BuildControl()
        {
            if (!string.IsNullOrEmpty(this.description.GameText))
            {
                var labelBuilder = this.formattedLabelHtmlService.CreateLabel(this.description.GameText)
                    .AutoSizeHeight()
                    .SetWidth(this.ContentRegion.Width)
                    .Wrap();

                this.gameTextLabel = labelBuilder.Build();
                this.gameTextLabel.Parent = this;
            }

            if (!string.IsNullOrEmpty(this.description.GameHint))
            {
                var labelBuilder = this.formattedLabelHtmlService.CreateLabel(this.description.GameHint)
                    .AutoSizeHeight()
                    .SetWidth(this.ContentRegion.Width)
                    .Wrap();

                this.gameHintLabel = labelBuilder.Build();
                this.gameHintLabel.Parent = this;
            }

            this.panel = new FlowPanel()
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
                        Width = 39,
                        Height = 39,
                    };

                    var control = this.CreateEntryControl(i, entries[i], imagePanel);

                    control.Location = new Point((imagePanel.Width - control.Width) / 2, (imagePanel.Height - control.Height) / 2);
                    var tooltipControl = control;

                    if (control is ImageSpinner imageSpinner)
                    {
                        tooltipControl = imageSpinner.Image;
                    }

                    tooltipControl.Tooltip = new Tooltip()
                    {
                        HeightSizingMode = SizingMode.AutoSize,
                        WidthSizingMode = SizingMode.AutoSize,
                    };

                    var tooltipPanel = new FlowPanel()
                    {
                        Parent = tooltipControl.Tooltip,
                        Width = 100,
                        HeightSizingMode = SizingMode.AutoSize,
                        FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    };

                    _ = new Label()
                    {
                        Parent = tooltipPanel,
                        Width = tooltipPanel.ContentRegion.Width,
                        WrapText = true,
                        AutoSizeHeight = true,
                        Font = Content.DefaultFont18,
                        Text = this.GetDisplayName(entries[i]),
                    };

                    this.ColorControl(control, finishedAchievement || this.AchievementService.HasFinishedAchievementBit(this.achievement.Id, i));

                    if (this.achievementDetails != null)
                    {
                        var index = i;
                        control.Click += (s, eventArgs)
                            => this.itemDetailWindowManager.CreateAndShowWindow(
                                this.GetDisplayName(entries[index]),
                                this.achievementDetails.ColumnNames,
                                this.achievementDetails.Entries[index],
                                this.achievementDetails.Link,
                                this.achievementDetails.Id,
                                index);
                    }
                    else
                    {
                        if (entries[i] is ILinkEntry linkEntry)
                        {
                            control.Click += (s, eventArgs)
                                => _ = System.Diagnostics.Process.Start("https://wiki.guildwars2.com" + linkEntry.Link);
                        }
                    }

                    this.itemControls.Add(control);
                }
            });
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            if (this.gameTextLabel != null)
            {
                this.gameTextLabel.Width = this.ContentRegion.Width;
            }

            if (this.gameHintLabel != null)
            {
                this.gameHintLabel.Width = this.ContentRegion.Width;
            }

            this.panel.Width = this.ContentRegion.Width;
            base.OnResized(e);
        }

        protected abstract IEnumerable<TEntry> GetEntries(T description);

        protected abstract Control CreateEntryControl(int index, TEntry entry, Container parent);

        protected abstract void ColorControl(Control control, bool achievementBitFinished);

        protected abstract string GetDisplayName(TEntry entry);

        protected override void DisposeControl()
        {
            foreach (var item in this.itemControls)
            {
                item.Dispose();
            }

            this.itemControls.Clear();

            base.DisposeControl();
        }
    }
}
