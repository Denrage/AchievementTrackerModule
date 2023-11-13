using System;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IBlishTabNavigationService : IDisposable
    {
        void Initialize();
        void NavigateToAchievementTab();
    }
}
