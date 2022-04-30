using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableItemEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableItemEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableItemEntry entry)
            => new Label()
            {
                Text = StringUtils.SanitizeHtml(entry.Name),
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
