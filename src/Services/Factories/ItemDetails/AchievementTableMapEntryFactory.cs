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
            var panel = new Panel()
            {
                Width = 250,
                Height = 250,
            };

            var spinner = new LoadingSpinner()
            {
                Location = new Microsoft.Xna.Framework.Point(panel.Width / 2, panel.Height / 2),
                Parent = panel,
            };

            spinner.Show();

            var result = new Image()
            {
                Parent = panel,
                Texture = this.achievementService.GetImageFromIndirectLink(entry.ImageLink, () => spinner.Dispose()),
                Size = panel.ContentRegion.Size,
            };

            panel.Resized += (s,e) => result.Size = panel.ContentRegion.Size;

            result.LeftMouseButtonReleased += (o, e) 
                => _ = Task.Run(async () => _ = System.Diagnostics.Process.Start("https://wiki.guildwars2.com" + await this.achievementService.GetDirectImageLink(entry.ImageLink)));

            return panel;
        }
    }
}
 