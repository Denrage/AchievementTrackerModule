using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableLinkEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableLinkEntry>
    {
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;

        public AchievementTableLinkEntryFactory(IFormattedLabelHtmlService formattedLabelHtmlService)
        {
            this.formattedLabelHtmlService = formattedLabelHtmlService;
        }

        protected override Control CreateInternal(CollectionAchievementTableLinkEntry entry)
            => this.formattedLabelHtmlService.CreateLabel(entry?.Text ?? string.Empty)
            .AutoSizeHeight()
            .Wrap()
            .Build();
    }
}
