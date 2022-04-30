using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTrackerService : IAchievementTrackerService, IDisposable
    {
        private readonly List<int> activeAchievements;
        private readonly IPersistanceService persistanceService;

        public event Action<int> AchievementTracked;

        public event Action<int> AchievementUntracked;

        public IReadOnlyList<int> ActiveAchievements => this.activeAchievements.AsReadOnly();

        public AchievementTrackerService(IPersistanceService persistanceService)
        {
            this.activeAchievements = new List<int>();
            this.persistanceService = persistanceService;
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

        public void Load()
        {
            foreach (var item in this.persistanceService.Get().TrackedAchievements)
            {
                this.activeAchievements.Add(item);
            }
        }

        public void Dispose()
        {
            foreach (var item in this.activeAchievements)
            {
                this.persistanceService.TrackAchievement(item);
            }
        }
    }
}
