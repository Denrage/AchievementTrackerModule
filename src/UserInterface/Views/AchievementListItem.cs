using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{
    public class AchievementListItem : View
    {
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly Achievement achievement;

        public AchievementListItem(Achievement achievement, IAchievementTrackerService achievementTrackerService)
        {
            this.achievement = achievement;
            this.achievementTrackerService = achievementTrackerService;
        }

        protected override void Build(Container buildPanel)
        {
            buildPanel.Click += (s, e) => this.achievementTrackerService.TrackAchievement(this.achievement);

            var button = new DetailsButton()
            {
                Text = this.achievement.Name,
                Parent = buildPanel,
                ShowToggleButton = true,
            };

            _ = new GlowButton()
            {
                Parent = button,
            };
        }
    }
}
