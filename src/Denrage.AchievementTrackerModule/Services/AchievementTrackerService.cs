using Blish_HUD;
using Blish_HUD.Settings;
using Denrage.AchievementTrackerModule.Interfaces;
using System;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTrackerService : IAchievementTrackerService
    {
        private readonly List<int> activeAchievements;
        private readonly Logger logger;
        private readonly SettingEntry<bool> limitAchievement;

        public event Action<int> AchievementTracked;

        public event Action<int> AchievementUntracked;

        public IReadOnlyList<int> ActiveAchievements => this.activeAchievements.AsReadOnly();

        public AchievementTrackerService(Logger logger, SettingEntry<bool> limitAchievement)
        {
            this.activeAchievements = new List<int>();
            this.logger = logger;
            this.limitAchievement = limitAchievement;
        }

        public bool TrackAchievement(int achievement)
        {
            if (!this.limitAchievement.Value || this.activeAchievements.Count <= 15)
            {
                if (!this.activeAchievements.Contains(achievement))
                {
                    this.activeAchievements.Add(achievement);
                    this.AchievementTracked?.Invoke(achievement);
                }
                return true;
            }

            return false;
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
