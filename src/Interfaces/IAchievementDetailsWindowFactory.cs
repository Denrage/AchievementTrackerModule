using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementDetailsWindowFactory
    {
        AchievementDetailsWindow Create(AchievementTableEntry achievement);
    }
}
