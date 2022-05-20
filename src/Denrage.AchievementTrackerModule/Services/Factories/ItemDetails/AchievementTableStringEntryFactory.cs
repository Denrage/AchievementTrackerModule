using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableStringEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableStringEntry>
    {
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;

        public AchievementTableStringEntryFactory(IFormattedLabelHtmlService formattedLabelHtmlService)
        {
            this.formattedLabelHtmlService = formattedLabelHtmlService;
        }

        protected override Control CreateInternal(CollectionAchievementTableStringEntry entry)
            => this.formattedLabelHtmlService.CreateLabel(entry?.Text ?? string.Empty)
            .AutoSizeHeight()
            .Wrap()
            .Build();
    }
}
