using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{
    public class AchievementListItem : View
    {
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly IAchievementService achievementService;
        private readonly ContentService contentService;
        private readonly string icon;
        private readonly AchievementTableEntry achievement;
        private DetailsButton button;

        public AchievementListItem(AchievementTableEntry achievement, IAchievementTrackerService achievementTrackerService, IAchievementService achievementService, ContentService contentService, string icon)
        {
            this.achievement = achievement;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementService = achievementService;
            this.contentService = contentService;
            this.icon = icon;

            this.achievementService.PlayerAchievementsLoaded += ()
                => this.ColorAchievement();
        }

        protected override void Build(Container buildPanel)
        {
            buildPanel.Click += (s, e) => this.achievementTrackerService.TrackAchievement(this.achievement.Id);

            this.button = new DetailsButton()
            {
                Text = this.achievement.Name,
                Parent = buildPanel,
                ShowToggleButton = true,
                Icon = this.contentService.GetRenderServiceTexture(this.icon),
            };

            this.ColorAchievement();
        }

        public void ColorAchievement()
        {
            if (this.button != null)
            {
                if (this.achievementService.HasFinishedAchievement(this.achievement.Id))
                {
                    this.button.BackgroundColor = Microsoft.Xna.Framework.Color.FromNonPremultiplied(144, 238, 144, 50);
                }
            }
        }
    }
}
