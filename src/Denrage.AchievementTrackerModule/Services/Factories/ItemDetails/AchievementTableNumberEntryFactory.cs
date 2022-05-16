using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableNumberEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableNumberEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableNumberEntry entry)
            => new Label()
            {
                Text = entry?.Number.ToString() ?? string.Empty,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
