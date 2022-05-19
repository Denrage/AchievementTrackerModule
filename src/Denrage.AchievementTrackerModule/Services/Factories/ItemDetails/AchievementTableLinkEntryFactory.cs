using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableLinkEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableLinkEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableLinkEntry entry)
            => Helper.FormattedLabelHelper.CreateLabel(entry?.Text ?? string.Empty)
            .AutoSizeHeight()
            .Wrap()
            .Build();
    }
}
