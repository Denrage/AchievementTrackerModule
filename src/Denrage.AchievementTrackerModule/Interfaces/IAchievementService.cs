using Denrage.AchievementTrackerModule.Libs.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementService
    {
        IReadOnlyList<AchievementTableEntry> Achievements { get; }

        IReadOnlyList<CollectionAchievementTable> AchievementDetails { get; }

        IEnumerable<AchievementGroup> AchievementGroups { get; }

        IEnumerable<AchievementCategory> AchievementCategories { get; }

        IReadOnlyList<SubPageInformation> Subpages { get; }

        event Action ApiAchievementsLoaded;
    }
}
