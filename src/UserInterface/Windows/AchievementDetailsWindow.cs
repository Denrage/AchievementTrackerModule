using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    // TODO: Add minimum and maximum size
    public class AchievementDetailsWindow : WindowBase2
    {
        private const int PADDING = 15;
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementControlManager achievementControlManager;
        private readonly AchievementTableEntry achievement;
        private readonly Texture2D texture;

        public AchievementDetailsWindow(
            ContentsManager contentsManager,
            AchievementTableEntry achievement,
            IAchievementService achievementService,
            IAchievementControlManager achievementControlManager)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementControlManager = achievementControlManager;
            this.achievement = achievement;

            this.texture = this.contentsManager.GetTexture("achievement_details_background.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            // TODO: Localization
            this.Title = "Achievement Details";
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 550, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 550, 600 - 30));

            var flowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = this.ContentRegion.Width - (PADDING * 2),
                Location = new Microsoft.Xna.Framework.Point(PADDING, 0),
                Height = this.ContentRegion.Height,
                CanScroll = true,
                Parent = this,
                ControlPadding = new Microsoft.Xna.Framework.Vector2(10f),
            };

            _ = new Label()
            {
                Text = this.achievement.Name,
                Parent = flowPanel,
                AutoSizeHeight = true,
                WrapText = true,
                Width = flowPanel.ContentRegion.Width,
                Font = Content.DefaultFont18,
                Padding = new Thickness(0, 0, 20, 0),
            };

            var panel = new Panel()
            {
                Width = flowPanel.ContentRegion.Width,
                Parent = flowPanel,
                HeightSizingMode = SizingMode.AutoSize,
            };

            this.achievementControlManager.ChangeParent(this.achievement.Id, panel);
        }

        public override void Hide()
        {
            this.achievementControlManager.RemoveParent(this.achievement.Id);
            base.Hide();
        }
    }
}
