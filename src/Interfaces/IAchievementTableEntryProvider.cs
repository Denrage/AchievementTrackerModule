using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Models.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementTableEntryProvider
    {
        Control GetTableEntryControl(CollectionAchievementTable.CollectionAchievementTableEntry entry);
    }
}
