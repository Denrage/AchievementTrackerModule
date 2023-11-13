using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementItemOverviewFactory : IAchievementItemOverviewFactory
    {
        private readonly IAchievementListItemFactory achievementListItemFactory;
        private readonly PlayerAchievementServiceFactory factory;
        private readonly IAchievementService achievementService;

        public AchievementItemOverviewFactory(IAchievementListItemFactory achievementListItemFactory, PlayerAchievementServiceFactory factory)
        {
            this.achievementListItemFactory = achievementListItemFactory;
            this.factory = factory;
        }

        public AchievementItemOverview Create(IEnumerable<(AchievementCategory, AchievementTableEntry)> achievements, string title)
            => new AchievementItemOverview(achievements, title, this.factory, this.achievementListItemFactory);
    }
}
