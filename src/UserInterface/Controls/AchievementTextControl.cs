using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementTextControl : FlowPanel, IAchievementControl
    {
        private readonly AchievementTableEntry achievement;
        private readonly StringDescription description;

        private Label gameTextLabel;
        private Label gameHintLabel;

        public AchievementTextControl(AchievementTableEntry achievement, StringDescription description)
        {
            this.achievement = achievement;
            this.description = description;

            this.FlowDirection = ControlFlowDirection.TopToBottom;
        }

        public void BuildControl()
        {
            if (!string.IsNullOrEmpty(this.description.GameText))
            {
                this.gameTextLabel = new Label()
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
                this.gameHintLabel = new Label()
                {
                    Parent = this,
                    Width = this.ContentRegion.Width,
                    Text = this.description.GameHint,
                    TextColor = Microsoft.Xna.Framework.Color.LightGray,
                    AutoSizeHeight = true,
                    WrapText = true,
                };
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
