using Gw2Sharp.WebApi.V2.Models;
using System;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementTrackerService
    {
        event Action<Achievement> AchievementTracked;

        void RemoveAchievement(Achievement achievement);

        void TrackAchievement(Achievement achievement);
    }
}
