using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Flurl.Http;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly ContentsManager contentsManager;
        private readonly Gw2ApiManager gw2ApiManager;

        public IEnumerable<AccountAchievement> PlayerAchievements { get; private set; }

        public IReadOnlyList<Models.Achievement.AchievementTableEntry> Achievements { get; private set; }

        public IReadOnlyList<Models.Achievement.CollectionAchievementTable> AchievementDetails { get; private set; }

        public event Action PlayerAchievementsLoaded;

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
                this.Achievements = (await JsonSerializer.DeserializeAsync<List<Models.Achievement.AchievementTableEntry>>(achievements, serializerOptions)).AsReadOnly();
            }

            using (var achievementDetails = this.contentsManager.GetFileStream("achievement_tables.json"))
            {
                this.AchievementDetails = (await JsonSerializer.DeserializeAsync<List<Models.Achievement.CollectionAchievementTable>>(achievementDetails, serializerOptions)).AsReadOnly();
            }

            await this.LoadPlayerAchievements();
        }

        public bool HasFinishedAchievement(int achievementId)
        {
            if (this.PlayerAchievements is null)
            {
                return false;
            }

            var achievement = this.PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);

            return !(achievement is null) && achievement.Done;
        }

        public bool HasFinishedAchievementBit(int achievementId, int positionIndex)
        {
            if (this.PlayerAchievements is null)
            {
                return false;
            }

            var achievement = this.PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);

            return !(achievement is null) && (achievement.Bits?.Contains(positionIndex) ?? false);
        }

        public async Task LoadPlayerAchievements()
        {
            if (this.PlayerAchievements == null && this.gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
            {
                this.PlayerAchievements = await this.gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync();
                _ = Task.Run(() => this.PlayerAchievementsLoaded?.Invoke());
            }
        }

        public AsyncTexture2D GetImage(string imageUrl)
            => this.GetImageInternal(async () => await this.DownloadWikiContent(imageUrl).GetStreamAsync());

        public async Task<string> GetDirectImageLink(string imagePath)
        {
            if (imagePath.Contains("File:"))
            {
                var source = await this.DownloadWikiContent(imagePath).GetStringAsync();

                var fillImageStartIndex = source.IndexOf("fullImageLink");
                var hrefStartIndex = source.IndexOf("href=", fillImageStartIndex);
                var linkStartIndex = source.IndexOf("\"", hrefStartIndex) + 1;
                var linkEndIndex = source.IndexOf("\"", linkStartIndex);
                var link = source.Substring(linkStartIndex, linkEndIndex - linkStartIndex);

                return link;
            }

            return imagePath;
        }

        public AsyncTexture2D GetImageFromIndirectLink(string imagePath) 
            => _ = this.GetImageInternal(async () => await this.DownloadWikiContent(await this.GetDirectImageLink(imagePath)).GetStreamAsync());

        private AsyncTexture2D GetImageInternal(Func<Task<Stream>> getImageStream)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);

            _ = Task.Run(async () =>
            {
                var imageStream = await getImageStream();

                texture.SwapTexture(TextureUtil.FromStreamPremultiplied(imageStream));
                imageStream.Close();
            });

            return texture;
        }

        private IFlurlRequest DownloadWikiContent(string url)
            => ("https://wiki.guildwars2.com" + url)
                    .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36");

    }
}
