using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementDetailsWindowFactory
    {
        AchievementDetailsWindow Create(AchievementTableEntry achievement);
    }
}
