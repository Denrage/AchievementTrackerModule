using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableLinkEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableLinkEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableLinkEntry entry)
            => new Label()
            {
                Text = entry?.Text ?? string.Empty,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
