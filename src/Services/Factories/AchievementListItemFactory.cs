using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
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
}
