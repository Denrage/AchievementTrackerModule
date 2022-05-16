using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableCoinEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableCoinEntry>
    {
        // TODO: Get prices from TradingPost
        protected override Control CreateInternal(CollectionAchievementTableCoinEntry entry)
            => new Label()
            {
                Text = (entry?.ItemId.ToString() ?? string.Empty) + ": " + (entry?.Type.ToString() ?? string.Empty),
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
