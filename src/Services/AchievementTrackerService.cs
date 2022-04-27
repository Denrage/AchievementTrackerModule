using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTrackerService : IAchievementTrackerService
    {
        private readonly List<int> activeAchievements;

        public event Action<int> AchievementTracked;

        public event Action<int> AchievementUntracked;

        public IReadOnlyList<int> ActiveAchievements => this.activeAchievements.AsReadOnly();

        public AchievementTrackerService()
        {
            this.activeAchievements = new List<int>();
        }

        public void TrackAchievement(int achievement)
        {
            this.activeAchievements.Add(achievement);
            this.AchievementTracked?.Invoke(achievement);
        }

        public void RemoveAchievement(int achievement)
        {
            _ = this.activeAchievements.Remove(achievement);
            this.AchievementUntracked?.Invoke(achievement);
        }
    }
}
