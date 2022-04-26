using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTrackerService : IAchievementTrackerService
    {
        private readonly List<Achievement> activeAchievements;

        public event Action<Achievement> AchievementTracked;

        public event Action<Achievement> AchievementUntracked;

        public IReadOnlyList<Achievement> ActiveAchievements => this.activeAchievements.AsReadOnly();

        public AchievementTrackerService()
        {
            this.activeAchievements = new List<Achievement>();
        }

        public void TrackAchievement(Achievement achievement)
        {
            this.activeAchievements.Add(achievement);
            this.AchievementTracked?.Invoke(achievement);
        }

        public void RemoveAchievement(Achievement achievement)
        {
            _ = this.activeAchievements.Remove(achievement);
            this.AchievementUntracked?.Invoke(achievement);
        }
    }
}
