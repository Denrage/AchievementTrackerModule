using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public abstract class AchievementTableEntryFactory<TEntry> : AchievementTableEntryFactory, ITableEntryFactory<TEntry>
        where TEntry : CollectionAchievementTableEntry
    {
        protected abstract Control CreateInternal(TEntry entry);

        public override Control Create(object entry)
            => this.Create((TEntry)entry);

        public Control Create(TEntry entry)
            => this.CreateInternal(entry);
    }
}
