using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementTrackerService
    {
        IReadOnlyList<int> ActiveAchievements { get; }

        event Action<int> AchievementTracked;

        event Action<int> AchievementUntracked;

        void RemoveAchievement(int achievement);

        void TrackAchievement(int achievement);
    }
}
