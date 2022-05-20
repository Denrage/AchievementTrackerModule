using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementTextControl : FlowPanel, IAchievementControl
    {
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;
        private readonly AchievementTableEntry achievement;
        private readonly StringDescription description;

        private FormattedLabel.FormattedLabel gameTextLabel;
        private FormattedLabel.FormattedLabel gameHintLabel;

        public AchievementTextControl(IFormattedLabelHtmlService formattedLabelHtmlService, AchievementTableEntry achievement, StringDescription description)
        {
            this.formattedLabelHtmlService = formattedLabelHtmlService;
            this.achievement = achievement;
            this.description = description;

            this.FlowDirection = ControlFlowDirection.TopToBottom;
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

            base.OnResized(e);
        }
    }
}
