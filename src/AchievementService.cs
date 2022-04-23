using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Flurl.Http;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    // TODO: Interface
    public class AchievementService
    {
        private readonly ContentsManager contentsManager;
        private readonly Gw2ApiManager gw2ApiManager;

        public IEnumerable<AccountAchievement> PlayerAchievements { get; private set; }

        public IReadOnlyList<Models.Achievement.AchievementTableEntry> Achievements { get; private set; }

        public IReadOnlyList<Models.Achievement.CollectionAchievementTable> AchievementDetails { get; private set; }

        public AchievementService(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager)
        {
            this.contentsManager = contentsManager;
            this.gw2ApiManager = gw2ApiManager;
        }

        public async Task LoadAsync()
        {
            var serializerOptions = new JsonSerializerOptions()
            {
                Converters = { new Models.Achievement.RewardConverter(), new Models.Achievement.AchievementTableEntryDescriptionConverter(), new Models.Achievement.CollectionAchievementTableEntryConverter() },
            };

            using (var achievements = this.contentsManager.GetFileStream("achievement_data.json"))
            {
                this.Achievements = (await System.Text.Json.JsonSerializer.DeserializeAsync<List<Models.Achievement.AchievementTableEntry>>(achievements, serializerOptions)).AsReadOnly();
            }

            using (var achievementDetails = this.contentsManager.GetFileStream("achievement_tables.json"))
            {
                this.AchievementDetails = (await System.Text.Json.JsonSerializer.DeserializeAsync<List<Models.Achievement.CollectionAchievementTable>>(achievementDetails, serializerOptions)).AsReadOnly();
            }

            await this.LoadPlayerAchievements();
        }

        public bool HasFinishedAchievement(int achievementId)
        {
            if (this.PlayerAchievements is null)
            {
                return false;
            }

            return this.PlayerAchievements.FirstOrDefault(x => x.Id == achievementId) != null;
        }

        public bool HasFinishedAchievementBit(int achievementId, int itemId)
        {
            if (this.PlayerAchievements is null)
            {
                return false;
            }

            var achievement = this.PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);

            if (achievement is null)
            {
                return false;
            }

            return achievement.Bits?.Contains(itemId) ?? false;
        }

        public async Task LoadPlayerAchievements()
        {
            if (this.PlayerAchievements == null && this.gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
            {
                this.PlayerAchievements = await this.gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync();
            }
        }

        public AsyncTexture2D GetImage(string imageUrl)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);

            Task.Run(async () =>
            {
                var imageStream = await ("https://wiki.guildwars2.com" + imageUrl).WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36").GetStreamAsync();


                GameService.Graphics.QueueMainThreadRender(_ =>
                {
                    texture.SwapTexture(TextureUtil.FromStreamPremultiplied(imageStream));
                    imageStream.Close();
                });
            });

            return texture;
        }

        // TODO: Merge with above
        public AsyncTexture2D GetDirectImageLink(string imagePath)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);

            Task.Run(async () =>
            {
                var source = await ("https://wiki.guildwars2.com" + imagePath).WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36").GetStringAsync();

                var fillImageStartIndex = source.IndexOf("fullImageLink");
                var hrefStartIndex = source.IndexOf("href=", fillImageStartIndex);
                var linkStartIndex = source.IndexOf("\"", hrefStartIndex) + 1;
                var linkEndIndex = source.IndexOf("\"", linkStartIndex);
                var link = source.Substring(linkStartIndex, linkEndIndex - linkStartIndex);

                var imageStream = await ("https://wiki.guildwars2.com" + link).WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36").GetStreamAsync();


                GameService.Graphics.QueueMainThreadRender(_ =>
                {
                    texture.SwapTexture(TextureUtil.FromStreamPremultiplied(imageStream));
                    imageStream.Close();
                });
            });

            return texture;
        }
    }

}
