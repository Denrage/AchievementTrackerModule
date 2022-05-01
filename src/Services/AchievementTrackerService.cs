using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTrackerService : IAchievementTrackerService
    {
        private readonly List<int> activeAchievements;
        private readonly Logger logger;

        public event Action<int> AchievementTracked;

        public event Action<int> AchievementUntracked;

        public IReadOnlyList<int> ActiveAchievements => this.activeAchievements.AsReadOnly();

        public AchievementTrackerService(Logger logger)
        {
            this.activeAchievements = new List<int>();
            this.logger = logger;
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

        public void Load(IPersistanceService persistanceService)
        {
            try
            {
                foreach (var item in persistanceService.Get().TrackedAchievements)
                {
                    this.activeAchievements.Add(item);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on restoring tracked achievements");
            }
        }
    }
}
