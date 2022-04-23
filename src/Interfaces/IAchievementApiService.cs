using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementApiService
    {
        IEnumerable<AchievementGroup> AchievementGroups { get; }

        IEnumerable<AchievementCategory> AchievementCategories { get; }
    }
}
