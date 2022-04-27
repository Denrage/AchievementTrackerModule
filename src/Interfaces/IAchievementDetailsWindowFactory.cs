using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementDetailsWindowFactory
    {
        AchievementDetailsWindow Create(AchievementTableEntry achievement);
    }
}
