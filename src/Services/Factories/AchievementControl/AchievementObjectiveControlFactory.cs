using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementObjectiveControlFactory : AchievementControlFactory<AchievementObjectivesControl, ObjectivesDescription>
    {
        private readonly IAchievementService achievementService;
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;

        public AchievementObjectiveControlFactory(IAchievementService achievementService, IItemDetailWindowFactory itemDetailWindowFactory)
        {
            this.achievementService = achievementService;
            this.itemDetailWindowFactory = itemDetailWindowFactory;
        }

        protected override AchievementObjectivesControl CreateInternal(Achievement achievement, ObjectivesDescription description)
            => new AchievementObjectivesControl(this.itemDetailWindowFactory, this.achievementService, achievement, description);
    }
}
