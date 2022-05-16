using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementItemOverviewFactory
    {
        AchievementItemOverview Create(IEnumerable<(AchievementCategory, AchievementTableEntry)> achievements, string title);
    }
}
