using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Controls.FormattedLabel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    public class ItemDetailWindow : WindowBase2
    {
        private const int PADDING = 15;

        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementTableEntryProvider achievementTableEntryProvider;
        private readonly ISubPageInformationWindowManager subPageInformationWindowManager;
        private readonly string achievementLink;
        private readonly string name;
        private readonly string[] columns;
        private readonly List<CollectionAchievementTableEntry> item;
        private readonly Texture2D texture;

        public ItemDetailWindow(
            ContentsManager contentsManager,
            IAchievementService achievementService,
            IAchievementTableEntryProvider achievementTableEntryProvider,
            ISubPageInformationWindowManager subPageInformationWindowManager,
            string achievementLink,
            string name,
            string[] columns,
            List<CollectionAchievementTableEntry> item)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementTableEntryProvider = achievementTableEntryProvider;
            this.subPageInformationWindowManager = subPageInformationWindowManager;
            this.achievementLink = achievementLink;
            this.texture = this.contentsManager.GetTexture("item_detail_background.png");

            this.name = name;
            this.columns = columns;
            this.item = item;

            this.BuildWindow();
        }

        private void BuildWindow()
        {
            // TODO: Localization
            this.Title = "Item Details";
            this.ConstructWindow(this.texture, new Rectangle(0, 0, 600, 400), new Rectangle(0, 30, 600, 400 - 30));

            var panel = new FlowPanel()
            {
                Parent = this,
                Size = this.ContentRegion.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = this.ContentRegion.Width - (PADDING * 2),
                Location = new Point(PADDING, 0),
                Height = this.ContentRegion.Height,
                ControlPadding = new Vector2(10f),
                CanScroll = true,
            };

            var item = this.item.OfType<CollectionAchievementTableItemEntry>().FirstOrDefault();
            var link = this.achievementLink;

            if (item != null)
            {
                link = item.Link;
            }

            var itemTitleBuilder = new FormattedLabelBuilder()
                .SetWidth(panel.ContentRegion.Width)
                .AutoSizeHeight()
                .Wrap();

            var itemTitlePart = itemTitleBuilder.CreatePart(this.name);

            _ = itemTitlePart.SetFontSize(Blish_HUD.ContentService.FontSize.Size18);

            if (!string.IsNullOrEmpty(link))
            {
                var inSubpages = false;
                foreach (var subPage in this.achievementService.Subpages)
                {
                    if (subPage.Link.Contains(link))
                    {
                        inSubpages = true;
                        _ = itemTitlePart.SetLink(() => this.subPageInformationWindowManager.Create(subPage)).MakeUnderlined();
                    }
                }

                if (!inSubpages)
                {
                    if (link.StartsWith("/"))
                    {
                        link = "https://wiki.guildwars2.com/" + link;
                    }

                    _ = itemTitlePart.SetHyperLink(link).MakeUnderlined();
                }
            }

            var itemTitle = itemTitleBuilder.CreatePart(itemTitlePart).Build();
            itemTitle.Parent = panel;

            for (var i = 0; i < this.item.Count; i++)
            {
                var innerPannel = new Panel()
                {
                    Parent = panel,
                    Width = panel.ContentRegion.Width,
                    HeightSizingMode = SizingMode.AutoSize,
                };

                var label = new Label()
                {
                    Parent = innerPannel,
                    Width = (int)System.Math.Floor(0.15 * innerPannel.ContentRegion.Width),
                    Text = this.columns[i],
                    WrapText = true,
                    AutoSizeHeight = true,
                };

                var control = this.achievementTableEntryProvider.GetTableEntryControl(this.item[i]);

                if (control != null)
                {
                    control.Parent = innerPannel;
                    control.Width = innerPannel.Width - label.Width - 15;
                    control.Location = new Point(label.Width, 0);
                }
            }
        }
    }
}
