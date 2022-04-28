using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementObjectiveControlFactory : AchievementControlFactory<AchievementObjectivesControl, ObjectivesDescription>
    {
        private readonly IAchievementService achievementService;
        private readonly IItemDetailWindowManager itemDetailWindowFactory;
        private readonly ContentsManager contentsManager;

        public AchievementObjectiveControlFactory(IAchievementService achievementService, IItemDetailWindowManager itemDetailWindowManager, ContentsManager contentsManager)
        {
            this.achievementService = achievementService;
            this.itemDetailWindowFactory = itemDetailWindowManager;
            this.contentsManager = contentsManager;
        }

        protected override AchievementObjectivesControl CreateInternal(AchievementTableEntry achievement, ObjectivesDescription description)
            => new AchievementObjectivesControl(this.itemDetailWindowFactory, this.achievementService, this.contentsManager, achievement, description);
    }
}
