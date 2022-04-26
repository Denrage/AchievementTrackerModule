using Blish_HUD.Modules.Managers;
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
        private readonly ContentsManager contentsManager;

        public AchievementCollectionControlFactory(IAchievementService achievementService, IItemDetailWindowFactory itemDetailWindowFactory, ContentsManager contentsManager)
        {
            this.achievementService = achievementService;
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.contentsManager = contentsManager;
        }

        protected override AchievementCollectionControl CreateInternal(Achievement achievement, CollectionDescription description)
            => new AchievementCollectionControl(this.itemDetailWindowFactory, this.achievementService, this.contentsManager, achievement, description);
    }
}
