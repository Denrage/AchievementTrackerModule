using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementCategoryOverviewFactory
    {
        AchievementCategoryOverview Create(AchievementCategory category);
    }
}
