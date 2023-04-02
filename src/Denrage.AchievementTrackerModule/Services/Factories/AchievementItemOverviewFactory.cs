using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Models;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementItemOverviewFactory : IAchievementItemOverviewFactory
    {
        private readonly IAchievementListItemFactory achievementListItemFactory;
        private readonly IAchievementService achievementService;

        public AchievementItemOverviewFactory(IAchievementListItemFactory achievementListItemFactory, IAchievementService achievementService)
        {
            this.achievementListItemFactory = achievementListItemFactory;
            this.achievementService = achievementService;
        }

        public AchievementItemOverview Create(IEnumerable<CategoryAchievements> achievements, string title)
            => new AchievementItemOverview(achievements, title, this.achievementService, this.achievementListItemFactory);
    }
}
