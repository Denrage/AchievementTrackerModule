using System;
using System.Collections.Generic;
using Gw2Sharp.WebApi.V2.Models;

namespace AchievementTrackerModule
{
    public interface IAchievementTrackerService
    {
        event Action<Achievement> AchievementTracked;

        void RemoveAchievement(Achievement achievement);

        void TrackAchievement(Achievement achievement);
    }

    public class AchievementTrackerService : IAchievementTrackerService
    {
        private readonly List<Achievement> activeAchievements;

        public event Action<Achievement> AchievementTracked;

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
            this.activeAchievements.Remove(achievement);
        }
    }

}
