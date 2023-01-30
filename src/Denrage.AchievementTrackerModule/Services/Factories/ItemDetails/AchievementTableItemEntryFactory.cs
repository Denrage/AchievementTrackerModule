using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableItemEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableItemEntry>
    {
        private readonly IExternalImageService externalImageService;

        public AchievementTableItemEntryFactory(IExternalImageService externalImageService)
        {
            this.externalImageService = externalImageService;
        }

        protected override Control CreateInternal(CollectionAchievementTableItemEntry entry)
        {
            var builder = new FormattedLabelBuilder();
            var partBuilder = builder.CreatePart(entry.Name);

            if (!string.IsNullOrEmpty(entry.ImageUrl))
            {
                _ = partBuilder.SetPrefixImage(this.externalImageService.GetImageFromIndirectLink(entry.ImageUrl));
            }

            return builder.CreatePart(partBuilder).Build();
        } }
}
