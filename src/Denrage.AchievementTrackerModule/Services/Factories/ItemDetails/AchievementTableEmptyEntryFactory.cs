using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableEmptyEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableEmptyEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableEmptyEntry entry)
            => new Label()
            {
                Text = "EMPTY",
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
