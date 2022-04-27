using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementListItemFactory : IAchievementListItemFactory
    {
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly ContentService contentService;

        public AchievementListItemFactory(IAchievementTrackerService achievementTrackerService, ContentService contentService)
        {
            this.achievementTrackerService = achievementTrackerService;
            this.contentService = contentService;
        }

        public AchievementListItem Create(Achievement achievement, string icon)
            => new AchievementListItem(achievement, this.achievementTrackerService, this.contentService, icon);
    }
}
