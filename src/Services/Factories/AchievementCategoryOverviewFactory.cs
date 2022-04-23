using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementCategoryOverviewFactory : IAchievementCategoryOverviewFactory
    {
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly IAchievementListItemFactory achievementListItemFactory;

        public AchievementCategoryOverviewFactory(Gw2ApiManager gw2ApiManager, IAchievementListItemFactory achievementListItemFactory)
        {
            this.gw2ApiManager = gw2ApiManager;
            this.achievementListItemFactory = achievementListItemFactory;
        }

        public AchievementCategoryOverview Create(AchievementCategory category)
            => new AchievementCategoryOverview(category, this.gw2ApiManager, this.achievementListItemFactory);
    }
}
