using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Models.Achievement;
using System;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementDetailsWindowManager
    {
        event Action<int> WindowHidden;

        void CreateWindow(AchievementTableEntry achievement);
        
        void Dispose();
        
        void Update();
    }
}