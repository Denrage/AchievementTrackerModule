using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Views;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementListItemFactory : IAchievementListItemFactory
    {
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly ContentService contentService;
        private readonly IAchievementService achievementService;

        public AchievementListItemFactory(IAchievementTrackerService achievementTrackerService, ContentService contentService, IAchievementService achievementService)
        {
            this.achievementTrackerService = achievementTrackerService;
            this.contentService = contentService;
            this.achievementService = achievementService;
        }

        public AchievementListItem Create(AchievementTableEntry achievement, string icon)
            => new AchievementListItem(achievement, this.achievementTrackerService, this.achievementService, this.contentService, icon);
    }
}
