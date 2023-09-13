using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models;
using Flurl.Http;
using Gw2Sharp.WebApi.V2.Models;
using MonoGame.Framework.Utilities.Deflate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementService : IAchievementService, IDisposable
    {
        private const string DataVersionUrl = "https://bhm.blishhud.com/Denrage.AchievementTrackerModule/data/version.json";
        private const string AchievementDataUrl = "https://bhm.blishhud.com/Denrage.AchievementTrackerModule/data/achievement_data.json";
        private const string AchievementTablesUrl = "https://bhm.blishhud.com/Denrage.AchievementTrackerModule/data/achievement_tables.json";
        private const string SubPagesUrl = "https://bhm.blishhud.com/Denrage.AchievementTrackerModule/data/subPages.json";
        private const string VersionFileName = "version.json";
        private const string AchievementDataFileName = "achievement_data.json";
        private const string AchievementTablesFileName = "achievement_tables.json";
        private const string SubPagesFileName = "subPages.json";

        private readonly ContentsManager contentsManager;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly Logger logger;
        private readonly DirectoriesManager directoriesManager;
        private readonly Func<IPersistanceService> getPersistanceService;
        private readonly ITextureService textureService;
        private Task trackAchievementProgressTask;
        private CancellationTokenSource trackAchievementProgressCancellationTokenSource;

        public Dictionary<int, List<int>> ManualCompletedAchievements { get; set; } = new Dictionary<int, List<int>>();

        public IEnumerable<AccountAchievement> PlayerAchievements { get; private set; }

        public IReadOnlyList<Libs.Achievement.AchievementTableEntry> Achievements { get; private set; }

        public IReadOnlyList<Libs.Achievement.CollectionAchievementTable> AchievementDetails { get; private set; }

        public IEnumerable<AchievementGroup> AchievementGroups { get; private set; }

        public IEnumerable<AchievementCategory> AchievementCategories { get; private set; }

        public IReadOnlyList<Libs.Achievement.SubPageInformation> Subpages { get; private set; }

        public event Action PlayerAchievementsLoaded;

        public event Action ApiAchievementsLoaded;

        public AchievementService(ContentsManager contentsManager, Gw2ApiManager gw2ApiManager, Logger logger, DirectoriesManager directoriesManager, Func<IPersistanceService> getPersistanceService, ITextureService textureService)
        {
            this.contentsManager = contentsManager;
            this.gw2ApiManager = gw2ApiManager;
            this.logger = logger;
            this.directoriesManager = directoriesManager;
            this.getPersistanceService = getPersistanceService;
            this.textureService = textureService;
        }

        public void ToggleManualCompleteStatus(int achievementId, int bit)
        {
            if (this.specialSnowflakeCompletedHandling.TryGetValue(achievementId, out var conversionFunc))
            {
                bit = conversionFunc(bit);
            }

            if (this.PlayerAchievements != null)
            {
                var achievement = this.PlayerAchievements.FirstOrDefault(x => x.Id == achievementId);
                if (achievement != null)
                {
                    if (achievement.Done)
                    {
                        return;
                    }

                    if (achievement.Bits.Contains(bit))
                    {
                        return;
                    }
                }
            }

            if (!this.ManualCompletedAchievements.TryGetValue(achievementId, out var achievementBits))
            {
                achievementBits = new List<int>();
                this.ManualCompletedAchievements[achievementId] = achievementBits;
            }

            if (achievementBits.Contains(bit))
            {
                _ = achievementBits.Remove(bit);
            }
            else
            {
                achievementBits.Add(bit);
            }

            this.PlayerAchievementsLoaded?.Invoke();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private bool CheckMd5(string md5ToCheck, string filePath)
        {
            using (var md5 = MD5.Create())
            using (var fileStream = System.IO.File.Open(filePath, FileMode.Open))
            {
                return md5ToCheck.Equals(ByteArrayToString(md5.ComputeHash(fileStream)), StringComparison.OrdinalIgnoreCase);
            }
        }

        private async Task<bool> DownloadFile(string url, string folder, string fileName, string md5)
        {
            var tries = 0;
            do
            {
                if (tries == 3)
                {
                    this.logger.Error("Couldn't download file, please download it manually! " + url);
                    return false;
                }
                _ = await url.DownloadFileAsync(folder, fileName);
                tries++;
            } while (!System.IO.File.Exists(Path.Combine(folder, fileName)) || !this.CheckMd5(md5, Path.Combine(folder, fileName)));

            return true;
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            this.logger.Info("Reading saved achievement information");
            var serializerOptions = new JsonSerializerOptions()
            {
                Converters = { new Libs.Achievement.RewardConverter(), new Libs.Achievement.AchievementTableEntryDescriptionConverter(), new Libs.Achievement.CollectionAchievementTableEntryConverter(), new Libs.Achievement.SubPageInformationConverter() },
            };

            try
            {
                var dataFolder = this.directoriesManager.GetFullDirectoryPath("achievement_module");
                _ = Directory.CreateDirectory(dataFolder);

                var downloadData = false;

                if (!System.IO.File.Exists(Path.Combine(dataFolder, VersionFileName)) ||
                    !System.IO.File.Exists(Path.Combine(dataFolder, AchievementDataFileName)) ||
                    !System.IO.File.Exists(Path.Combine(dataFolder, AchievementTablesFileName)) ||
                    !System.IO.File.Exists(Path.Combine(dataFolder, SubPagesFileName)))
                {
                    downloadData = true;
                }
                else
                {
                    var githubMetadata = await DataVersionUrl.GetJsonAsync<AchievementDataMetadata>();
                    using (var metadata = System.IO.File.Open(Path.Combine(dataFolder, VersionFileName), FileMode.Open))
                    {
                        var localMetadata = await JsonSerializer.DeserializeAsync<AchievementDataMetadata>(metadata, serializerOptions, cancellationToken);
                        if (localMetadata.Version != githubMetadata.Version)
                        {
                            downloadData = true;
                        }
                    }
                }

                if (downloadData)
                {
                    this.logger.Info("Downloading AchievementData");
                    _ = await DataVersionUrl.DownloadFileAsync(dataFolder, VersionFileName);
                    using (var metadata = System.IO.File.Open(Path.Combine(dataFolder, VersionFileName), FileMode.Open))
                    {
                        var localMetadata = await JsonSerializer.DeserializeAsync<AchievementDataMetadata>(metadata, serializerOptions, cancellationToken);

                        if(!await this.DownloadFile(AchievementDataUrl, dataFolder, AchievementDataFileName, localMetadata.AchievementDataMd5) ||
                        !await this.DownloadFile(AchievementTablesUrl, dataFolder, AchievementTablesFileName, localMetadata.AchievementTablesMd5) ||
                        !await this.DownloadFile(SubPagesUrl, dataFolder, SubPagesFileName, localMetadata.SubPagesMd5))
                        {
                            return;
                        }
                    }
                }

                using (var achievements = System.IO.File.Open(Path.Combine(dataFolder, AchievementDataFileName), FileMode.Open))
                {
                    this.Achievements = (await JsonSerializer.DeserializeAsync<List<Libs.Achievement.AchievementTableEntry>>(achievements, serializerOptions, cancellationToken)).AsReadOnly();
                }

                using (var achievementDetails = System.IO.File.Open(Path.Combine(dataFolder, AchievementTablesFileName), FileMode.Open))
                {
                    this.AchievementDetails = (await JsonSerializer.DeserializeAsync<List<Libs.Achievement.CollectionAchievementTable>>(achievementDetails, serializerOptions)).AsReadOnly();
                }

                using (var subpageInformation = System.IO.File.Open(Path.Combine(dataFolder, SubPagesFileName), FileMode.Open))
                {
                    this.Subpages = (await JsonSerializer.DeserializeAsync<List<Libs.Achievement.SubPageInformation>>(subpageInformation, serializerOptions)).AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on deserializing cached achievement data!");
                throw;
            }

            this.logger.Info("Finished reading saved achievement information");

            this.ManualCompletedAchievements = this.getPersistanceService().Get().ManualCompletedAchievements;

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

                foreach (var category in this.AchievementCategories)
                {
                    //Store texture
                    _ = this.textureService.GetTexture(category.Icon);
                }

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
            if (this.specialSnowflakeCompletedHandling.TryGetValue(achievementId, out var conversionFunc))
            {
                positionIndex = conversionFunc(positionIndex);
            }

            if (this.ManualCompletedAchievements.TryGetValue(achievementId, out var manualAchievement))
            {
                if (manualAchievement.Contains(positionIndex))
                {
                    return true;
                }
            }

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

                        foreach (var item in this.PlayerAchievements)
                        {
                            if (this.ManualCompletedAchievements.TryGetValue(item.Id, out var achievementBits))
                            {
                                if (item.Done)
                                {
                                    _ = this.ManualCompletedAchievements.Remove(item.Id);
                                }
                                else
                                {
                                    foreach (var bit in item.Bits)
                                    {
                                        if (achievementBits.Contains(bit))
                                        {
                                            _ = achievementBits.Remove(bit);
                                        }
                                    }
                                }
                            }
                        }

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

        public void Dispose()
            => this.trackAchievementProgressCancellationTokenSource.Cancel();

        private readonly Dictionary<int, Func<int, int>> specialSnowflakeCompletedHandling = new Dictionary<int, Func<int, int>>()
        {
            { 5693, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 6 : index == 4 ? 7 : index == 5 ? 8 : -1  },
            { 5700, index => index == 0 ? 1 : index == 1 ? 2 : index == 2 ? 5 : index == 3 ? 8 : -1  },
            { 5704, index => index == 0 ? 1 : index == 1 ? 2 : index == 2 ? 5 : index == 3 ? 8 : -1  },
            { 5703, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 3 : index == 4 ? 4 : index == 5 ? 5 : index == 6 ? 7 : -1  },
            { 5697, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 3 : index == 3 ? 5 : index == 4 ? 6 : -1  },
            { 5688, index => index == 0 ? 3 : index == 1 ? 4 : index == 2 ? 6 : index == 3 ? 7 : index == 4 ? 8 : -1  },
            { 5709, index => index == 0 ? 0 : index == 1 ? 2 : index == 2 ? 4 : index == 3 ? 6 : index == 4 ? 7 : -1  },
            { 5698, index => index == 0 ? 0 : index == 1 ? 3 : index == 2 ? 4 : index == 3 ? 6 : -1  },
            { 5691, index => index == 0 ? 4 : index == 1 ? 5 : index == 2 ? 6 : index == 3 ? 7 : -1  },
            { 5708, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 5 : index == 4 ? 8 : -1  },
        };
    }
}
