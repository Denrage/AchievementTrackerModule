using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementObjectiveControlFactory : AchievementControlFactory<AchievementObjectivesControl, ObjectivesDescription>
    {
        private readonly IAchievementService achievementService;
        private readonly IItemDetailWindowManager itemDetailWindowFactory;
        private readonly ContentsManager contentsManager;
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;
        private readonly IBitAlignmentService bitAlignmentService;

        public AchievementObjectiveControlFactory(IAchievementService achievementService, IItemDetailWindowManager itemDetailWindowManager, ContentsManager contentsManager, IFormattedLabelHtmlService formattedLabelHtmlService, IBitAlignmentService bitAlignmentService)
        {
            this.achievementService = achievementService;
            this.itemDetailWindowFactory = itemDetailWindowManager;
            this.contentsManager = contentsManager;
            this.formattedLabelHtmlService = formattedLabelHtmlService;
            this.bitAlignmentService = bitAlignmentService;
        }

        protected override AchievementObjectivesControl CreateInternal(AchievementTableEntry achievement, ObjectivesDescription description)
            => new AchievementObjectivesControl(this.itemDetailWindowFactory, this.achievementService, this.formattedLabelHtmlService, this.bitAlignmentService, this.contentsManager, achievement, description);
    }
}
