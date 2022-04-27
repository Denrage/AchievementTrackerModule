using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using System.Threading.Tasks;
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
        {
            var result = new Image()
            {
                Texture = this.achievementService.GetImageFromIndirectLink(entry.ImageLink),
                Width = 250,
                Height = 250,
            };

            result.LeftMouseButtonReleased += (o, e) 
                => _ = Task.Run(async () => _ = System.Diagnostics.Process.Start("https://wiki.guildwars2.com" + await this.achievementService.GetDirectImageLink(entry.ImageLink)));

            return result;
        }
    }
}
 