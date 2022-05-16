using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableStringEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableStringEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableStringEntry entry)
            => new Label()
            {
                Text = StringUtils.SanitizeHtml(entry?.Text ?? string.Empty),
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}
