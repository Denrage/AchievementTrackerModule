using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface ITableEntryFactory<TEntry>
        where TEntry : CollectionAchievementTableEntry
    {
        Control Create(TEntry entry);
    }
}
