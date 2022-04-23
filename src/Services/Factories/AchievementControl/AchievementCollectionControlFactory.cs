using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementCollectionControlFactory : AchievementControlFactory<AchievementCollectionControl, CollectionDescription>
    {
        private readonly IAchievementService achievementService;
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;

        public AchievementCollectionControlFactory(IAchievementService achievementService, IItemDetailWindowFactory itemDetailWindowFactory)
        {
            this.achievementService = achievementService;
            this.itemDetailWindowFactory = itemDetailWindowFactory;
        }

        protected override AchievementCollectionControl CreateInternal(Achievement achievement, CollectionDescription description)
            => new AchievementCollectionControl(this.itemDetailWindowFactory, this.achievementService, achievement, description);
    }
}
