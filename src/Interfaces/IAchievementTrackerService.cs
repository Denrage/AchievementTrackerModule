using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementTrackerService
    {
        IReadOnlyList<Achievement> ActiveAchievements { get; }

        event Action<Achievement> AchievementTracked;

        event Action<Achievement> AchievementUntracked;

        void RemoveAchievement(Achievement achievement);

        void TrackAchievement(Achievement achievement);
    }
}
