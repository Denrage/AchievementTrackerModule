using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableStringEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableStringEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableStringEntry entry)
            => new Label()
            {
                Text = entry.Text,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
