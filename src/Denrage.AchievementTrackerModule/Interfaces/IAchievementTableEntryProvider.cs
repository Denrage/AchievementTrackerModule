using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementTableEntryProvider
    {
        Control GetTableEntryControl(CollectionAchievementTable.CollectionAchievementTableEntry entry);
    }
}
