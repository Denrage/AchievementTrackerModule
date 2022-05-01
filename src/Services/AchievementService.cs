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
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementService : IAchievementService, IDisposable
    {
        private readonly ContentsManager contentsManager;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly Logger logger;
        private Task trackAchievementProgressTask;
        private CancellationTokenSource trackAchievementProgressCancellationTokenSource;

        public IEnumerable<AccountAchievement> PlayerAchievements { get; private set; }

        public IReadOnlyList<Models.Achievement.AchievementTableEntry> Achievements { get; private set; }

        public IReadOnlyList<Models.Achievement.CollectionAchievementTable> AchievementDetails { get; private set; }

        public IEnumerable<AchievementGroup> AchievementGroups { get; private set; }

        public IEnumerable<AchievementCategory> AchievementCategories { get; private set; }

        public event Action PlayerAchievementsLoaded;

        public event Action ApiAchievementsLoaded;

        public AchievementService(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager, Logger logger)
        {
            this.contentsManager = contentsManager;
            this.gw2ApiManager = gw2ApiManager;
            this.logger = logger;
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            this.logger.Info("Reading saved achievement information");
            var serializerOptions = new JsonSerializerOptions()
            {
                Converters = { new Models.Achievement.RewardConverter(), new Models.Achievement.AchievementTableEntryDescriptionConverter(), new Models.Achievement.CollectionAchievementTableEntryConverter() },
            };

            try
            {
                using (var achievements = this.contentsManager.GetFileStream("achievement_data.json"))
                {
                    this.Achievements = (await JsonSerializer.DeserializeAsync<List<Models.Achievement.AchievementTableEntry>>(achievements, serializerOptions, cancellationToken)).AsReadOnly();
                }

                using (var achievementDetails = this.contentsManager.GetFileStream("achievement_tables.json"))
                {
                    this.AchievementDetails = (await JsonSerializer.DeserializeAsync<List<Models.Achievement.CollectionAchievementTable>>(achievementDetails, serializerOptions)).AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on deserializing cached achievement data!");
                throw;
            }

            this.logger.Info("Finished reading saved achievement information");

            _ = Task.Run(async () => await this.InitializeApiAchievements());

            await this.LoadPlayerAchievements(cancellationToken: cancellationToken);
        }

        private async Task InitializeApiAchievements(CancellationToken cancellationToken = default)
        {
            this.logger.Info("Getting achievement data from api");

            try
            {
                this.AchievementGroups = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.Groups.AllAsync(cancellationToken);
                this.AchievementCategories = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.Categories.AllAsync(cancellationToken);
                this.logger.Info("Finished getting achievement data from api");

                this.ApiAchievementsLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed getting api achievements. Retrying in 5 minutes");
                await Task.Delay(TimeSpan.FromMinutes(5));
                _ = Task.Run(async () => await this.InitializeApiAchievements());
            }
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

        public async Task LoadPlayerAchievements(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            if (forceRefresh || this.PlayerAchievements == null)
            {
                if (this.gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
                {
                    this.logger.Info("Refreshing Player Achievements");
                    try
                    {
                        this.PlayerAchievements = await this.gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync(cancellationToken);
                        _ = Task.Run(() => this.PlayerAchievementsLoaded?.Invoke(), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex, "Exception occured during refresh of player achievements. Skipping this time.");
                    }

                    this.TrackAchievementProgress();
                }
                else
                {
                    this.logger.Info("Permissions not granted");
                }
            }
        }

        private void TrackAchievementProgress()
        {
            if (this.trackAchievementProgressTask != null)
            {
                return;
            }

            this.trackAchievementProgressCancellationTokenSource = new CancellationTokenSource();
            this.trackAchievementProgressTask = Task.Run(this.TrackAchievementProgressMethod);
        }

        private async Task TrackAchievementProgressMethod()
        {
            try
            {
                while (true)
                {
                    this.trackAchievementProgressCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromMinutes(5), this.trackAchievementProgressCancellationTokenSource.Token);
                    await this.LoadPlayerAchievements(true, this.trackAchievementProgressCancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            { /* NOOP */ }
        }

        public AsyncTexture2D GetImage(string imageUrl, Action beforeSwap)
            => this.GetImageInternal(async () => await this.DownloadWikiContent(imageUrl).GetStreamAsync(), beforeSwap);

        public async Task<string> GetDirectImageLink(string imagePath, CancellationToken cancellationToken = default)
        {
            if (imagePath.Contains("File:"))
            {
                try
                {
                    var source = await this.DownloadWikiContent(imagePath).GetStringAsync(cancellationToken);

                    var fillImageStartIndex = source.IndexOf("fullImageLink");
                    var hrefStartIndex = source.IndexOf("href=", fillImageStartIndex);
                    var linkStartIndex = source.IndexOf("\"", hrefStartIndex) + 1;
                    var linkEndIndex = source.IndexOf("\"", linkStartIndex);
                    var link = source.Substring(linkStartIndex, linkEndIndex - linkStartIndex);
                    return link;
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Exception occured on parsing an image path");
                    return string.Empty;
                }
            }

            return imagePath;
        }

        public AsyncTexture2D GetImageFromIndirectLink(string imagePath, Action beforeSwap)
            => _ = this.GetImageInternal(async () => await this.DownloadWikiContent(await this.GetDirectImageLink(imagePath)).GetStreamAsync(), beforeSwap);

        private AsyncTexture2D GetImageInternal(Func<Task<Stream>> getImageStream, Action beforeSwap)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);

            _ = Task.Run(async () =>
            {
                try
                {
                    var imageStream = await getImageStream();

                    beforeSwap();

                    GameService.Graphics.QueueMainThreadRender(device =>
                    {
                        texture.SwapTexture(TextureUtil.FromStreamPremultiplied(device, imageStream));
                        imageStream.Close();
                    });
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Exception occured on downloading/swapping image");

                    GameService.Graphics.QueueMainThreadRender(_ => texture.SwapTexture(ContentService.Textures.Error));
                }
            });

            return texture;
        }

        private IFlurlRequest DownloadWikiContent(string url)
            => ("https://wiki.guildwars2.com" + url)
                    .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36");

        public void Dispose()
            => this.trackAchievementProgressCancellationTokenSource.Cancel();
    }
}
