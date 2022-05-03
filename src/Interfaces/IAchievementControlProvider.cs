using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Models.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementControlProvider
    {
        Control GetAchievementControl(AchievementTableEntry achievement, AchievementTableEntryDescription description);
    }
}
