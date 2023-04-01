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
        private readonly ITextureService textureService;

        public AchievementListItemFactory(IAchievementTrackerService achievementTrackerService, ContentService contentService, IAchievementService achievementService, ITextureService textureService)
        {
            this.achievementTrackerService = achievementTrackerService;
            this.contentService = contentService;
            this.achievementService = achievementService;
            this.textureService = textureService;

        }

        public AchievementListItem Create(AchievementTableEntry achievement, string icon)
            => new AchievementListItem(achievement, this.achievementTrackerService, this.achievementService, this.contentService, this.textureService, icon);
    }
}
