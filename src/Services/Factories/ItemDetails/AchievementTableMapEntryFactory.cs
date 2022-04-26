using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableMapEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableMapEntry>
    {
        private readonly IAchievementService achievementService;

        public AchievementTableMapEntryFactory(IAchievementService achievementService)
        {
            this.achievementService = achievementService;
        }

        protected override Control CreateInternal(CollectionAchievementTableMapEntry entry)
            => new Image()
            {
                Texture = this.achievementService.GetDirectImageLink(entry.ImageLink),
                Width = 250,
                Height = 250,
            };
    }
}
 