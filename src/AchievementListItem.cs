using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule
{
    public interface IAchievementListItemFactory
    {
        AchievementListItem Create(Achievement achievement);
    }

    public class AchievementListItemFactory : IAchievementListItemFactory
    {
        private readonly IAchievementTrackerService achievementTrackerService;

        public AchievementListItemFactory(IAchievementTrackerService achievementTrackerService)
        {
            this.achievementTrackerService = achievementTrackerService;
        }

        public AchievementListItem Create(Achievement achievement)
            => new AchievementListItem(achievement, this.achievementTrackerService);
    }

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

            new GlowButton()
            {
                Parent = button,
            };
        }
    }

}
