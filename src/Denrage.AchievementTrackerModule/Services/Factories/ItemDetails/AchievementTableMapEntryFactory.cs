using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Controls;
using System.Threading.Tasks;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableMapEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableMapEntry>
    {
        private readonly IExternalImageService externalImageService;
        private readonly Logger logger;

        public AchievementTableMapEntryFactory(IExternalImageService externalImageService, Logger logger)
        {
            this.externalImageService = externalImageService;
            this.logger = logger;
        }

        protected override Control CreateInternal(CollectionAchievementTableMapEntry entry)
        {
            var result = new ImageSpinner(this.externalImageService.GetImageFromIndirectLink(entry.ImageLink))
            {
                Width = 250,
                Height = 250,
                ZIndex = 1,
            };

            result.LeftMouseButtonReleased += (o, e)
                => _ = Task.Run(async () =>
                {
                    try
                    {
                        _ = System.Diagnostics.Process.Start("https://wiki.guildwars2.com" + await this.externalImageService.GetDirectImageLink(entry.ImageLink));
                    }
                    catch (System.Exception ex)
                    {
                        this.logger.Error(ex, "Exception occured on opening map in browser");
                    }
                });

            return result;
        }
    }
}
