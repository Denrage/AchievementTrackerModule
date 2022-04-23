using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableCoinEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableCoinEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableCoinEntry entry)
            => new Label()
            {
                Text = entry.ItemId + ": " + entry.Type.ToString(),
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
