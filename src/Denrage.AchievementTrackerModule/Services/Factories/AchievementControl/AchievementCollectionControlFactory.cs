using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementCollectionControlFactory : AchievementControlFactory<AchievementCollectionControl, CollectionDescription>
    {
        private readonly IAchievementService achievementService;
        private readonly IItemDetailWindowManager itemDetailWindowManager;
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;
        private readonly IBitAlignmentService bitAlignmentService;
        private readonly ContentsManager contentsManager;
        private readonly IExternalImageService externalImageService;

        public AchievementCollectionControlFactory(IAchievementService achievementService, IItemDetailWindowManager itemDetailWindowManager, IFormattedLabelHtmlService formattedLabelHtmlService, IBitAlignmentService bitAlignmentService, ContentsManager contentsManager, IExternalImageService externalImageService)
        {
            this.achievementService = achievementService;
            this.itemDetailWindowManager = itemDetailWindowManager;
            this.formattedLabelHtmlService = formattedLabelHtmlService;
            this.bitAlignmentService = bitAlignmentService;
            this.contentsManager = contentsManager;
            this.externalImageService = externalImageService;
        }

        protected override AchievementCollectionControl CreateInternal(AchievementTableEntry achievement, CollectionDescription description)
            => new AchievementCollectionControl(this.itemDetailWindowManager, this.achievementService, this.formattedLabelHtmlService, this.bitAlignmentService, this.externalImageService, this.contentsManager, achievement, description);
    }
}
