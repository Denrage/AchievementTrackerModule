using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Controls.FormattedLabel;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableItemEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableItemEntry>
    {
        private readonly IAchievementService achievementService;

        public AchievementTableItemEntryFactory(IAchievementService achievementService)
        {
            this.achievementService = achievementService;
        }

        protected override Control CreateInternal(CollectionAchievementTableItemEntry entry)
        {
            var builder = new FormattedLabelBuilder();
            var partBuilder = builder.CreatePart(entry.Name);

            if (!string.IsNullOrEmpty(entry.ImageUrl))
            {
                _ = partBuilder.SetPrefixImage(this.achievementService.GetImageFromIndirectLink(entry.ImageUrl, null));
            }

            return builder.CreatePart(partBuilder).Build();
        } }
}
