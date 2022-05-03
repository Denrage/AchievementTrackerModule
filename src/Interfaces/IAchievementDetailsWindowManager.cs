using Denrage.AchievementTrackerModule.Models.Achievement;
using System;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementDetailsWindowManager : IDisposable
    {
        event Action<int> WindowHidden;

        void CreateWindow(AchievementTableEntry achievement);

        void Update();
        bool WindowExists(int achievementId);
    }
}