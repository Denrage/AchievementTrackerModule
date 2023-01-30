using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Libs.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    internal class SubPageInformationWindow : WindowBase2
    {
        private const int PADDING = 15;

        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;
        private readonly SubPageInformation subPageInformation;
        private readonly IExternalImageService externalImageService;
        private readonly Texture2D texture;

        public SubPageInformationWindow(ContentsManager contentsManager, IAchievementService achievementService, IFormattedLabelHtmlService formattedLabelHtmlService, SubPageInformation subPageInformation, IExternalImageService externalImageService)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.formattedLabelHtmlService = formattedLabelHtmlService;
            this.subPageInformation = subPageInformation;
            this.externalImageService = externalImageService;
            this.texture = this.contentsManager.GetTexture("subpage_background.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            var title = subPageInformation.Title.Substring(0, System.Math.Min(subPageInformation.Title.Length, 25));
            if (title != subPageInformation.Title)
            {
                title += " ...";
            }

            this.Title = title;
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 550, 400), new Microsoft.Xna.Framework.Rectangle(0, 30, 550, 400 - 30));

            var flowPanel = new FlowPanel()
            {
                Parent = this,
                Width = this.ContentRegion.Width,
                Height = this.ContentRegion.Height,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Microsoft.Xna.Framework.Vector2(0, 20),
                OuterControlPadding = new Microsoft.Xna.Framework.Vector2(PADDING, PADDING),
            };

            var titleLabel = new FormattedLabelBuilder()
                .CreatePart(this.subPageInformation.Title, x => x.SetHyperLink(this.subPageInformation.Link).SetFontSize(Blish_HUD.ContentService.FontSize.Size24).MakeUnderlined())
                .CreatePart("  ", x => x.SetSuffixImage(this.contentsManager.GetTexture("wiki.png")).SetSuffixImageSize(new Microsoft.Xna.Framework.Point(24, 24)))
                .AutoSizeHeight()
                .SetWidth(flowPanel.ContentRegion.Width)
                .Wrap()
                .Build();
            
            titleLabel.Parent = flowPanel;

            var panel = new Panel()
            {
                Parent = flowPanel,
                Width = flowPanel.ContentRegion.Width - (PADDING * 3),
                HeightSizingMode = SizingMode.AutoSize,
            };

            var labelWidth = panel.ContentRegion.Width;

            if (this.subPageInformation is IHasDescriptionList)
            {
                labelWidth = panel.ContentRegion.Width / 2;
            }

            var labelBuild = this.formattedLabelHtmlService.CreateLabel(this.subPageInformation.Description)
                .AutoSizeHeight()
                .SetWidth(labelWidth - 5)
                .Wrap();

            var label = labelBuild.Build();
            label.Parent = panel;

            Control statisticsControl = null;
            if (this.subPageInformation is LocationSubPageInformation locationSubPage)
            {
                var statisticsLabelBuilds = this.formattedLabelHtmlService.CreateLabel(locationSubPage.Statistics)
                    .AutoSizeHeight()
                    .Wrap()
                    .SetWidth((panel.ContentRegion.Width / 2) - 5)
                    .SetHorizontalAlignment(HorizontalAlignment.Center);

                var statisticsLabel = statisticsLabelBuilds.Build();
                statisticsLabel.Location = new Microsoft.Xna.Framework.Point(labelWidth + 5, 0);
                statisticsLabel.Parent = panel;
                statisticsControl = statisticsLabel;
            }

            Control imageControl = null;

            if (subPageInformation is IHasImage hasImage)
            {
                if (!string.IsNullOrEmpty(hasImage.ImageUrl))
                {
                    var imageSpinner = new ImageSpinner(this.externalImageService.GetImageFromIndirectLink(hasImage.ImageUrl))
                    {
                        Parent = panel,
                        Width = (panel.ContentRegion.Width / 2) - 5,
                        Height = 200,
                        Location = new Microsoft.Xna.Framework.Point(labelWidth + 5, 0),
                    };

                    if (statisticsControl != null)
                    {
                        imageSpinner.Location = new Microsoft.Xna.Framework.Point(labelWidth + 5, statisticsControl.Height + 5);
                    }

                    imageControl = imageSpinner;
                }
            }

            if (this.subPageInformation is IHasDescriptionList descriptionList)
            {
                var descriptionListPanel = new FlowPanel()
                {
                    HeightSizingMode = SizingMode.AutoSize,
                    FlowDirection = ControlFlowDirection.SingleTopToBottom,
                    Width = panel.ContentRegion.Width / 2,
                    Location = new Microsoft.Xna.Framework.Point(labelWidth + 5, 0),
                    ControlPadding = new Microsoft.Xna.Framework.Vector2(0, 30),
                    Parent = panel,
                };

                foreach (var item in descriptionList.DescriptionList)
                {
                    var descriptionEntryPanel = new Panel()
                    {
                        Width = descriptionListPanel.ContentRegion.Width,
                        HeightSizingMode= SizingMode.AutoSize,
                        Parent = descriptionListPanel,
                    };
                    var labelBuilder = this.formattedLabelHtmlService.CreateLabel(item.Key)
                        .AutoSizeHeight()
                        .Wrap()
                        .SetWidth(descriptionEntryPanel.ContentRegion.Width / 2);

                    var keyLabel = labelBuilder.Build();
                    keyLabel.Parent = descriptionEntryPanel;

                    labelBuilder = this.formattedLabelHtmlService.CreateLabel(item.Value)
                        .AutoSizeHeight()
                        .Wrap()
                        .SetWidth(descriptionEntryPanel.ContentRegion.Width / 2);

                    var valueLabel = labelBuilder.Build();
                    valueLabel.Parent = descriptionEntryPanel;
                    valueLabel.Location = new Microsoft.Xna.Framework.Point(descriptionEntryPanel.ContentRegion.Width / 2, 0);
                }

                if (imageControl != null)
                {
                    descriptionListPanel.Location = new Microsoft.Xna.Framework.Point(labelWidth + 5, imageControl.Location.Y + imageControl.Height + 5);
                }
            }

            if (this.subPageInformation is ItemSubPageInformation itemSubPage)
            {
                var acquisitionLabelBuilder = this.formattedLabelHtmlService.CreateLabel(itemSubPage.Acquisition)
                    .AutoSizeHeight()
                    .Wrap()
                    .SetWidth(flowPanel.ContentRegion.Width - (PADDING * 3));

                var acquisitionLabel = acquisitionLabelBuilder.Build();
                acquisitionLabel.Parent = flowPanel;
            }

            if (this.subPageInformation is IHasAdditionalImages additionalImages)
            {
                var additionalImagesFlowPanel = new FlowPanel()
                {
                    Parent = flowPanel,
                    Width = flowPanel.ContentRegion.Width - (PADDING * 3),
                    FlowDirection = ControlFlowDirection.LeftToRight,
                    HeightSizingMode = SizingMode.AutoSize,
                };

                foreach (var item in additionalImages.AdditionalImages)
                {
                    _ = new ImageSpinner(this.externalImageService.GetImageFromIndirectLink(item))
                    {
                        Parent = additionalImagesFlowPanel,
                        Width = additionalImagesFlowPanel.Width / 3,
                        Height = 100,
                    };
                }
            }

            if (this.subPageInformation is IHasInteractiveMap interactiveMap)
            {
                if (interactiveMap.InteractiveMap != null)
                {
                    _ = new InteractiveMapControl(interactiveMap.InteractiveMap.IconUrl, interactiveMap.InteractiveMap.LocalTiles, interactiveMap.InteractiveMap.Coordinates, interactiveMap.InteractiveMap.Path, interactiveMap.InteractiveMap.Bounds)
                    {
                        Parent = flowPanel,
                        Width = flowPanel.ContentRegion.Width - (PADDING * 3),
                        Height = 400,
                    };
                }
            }
        }
    }
}
