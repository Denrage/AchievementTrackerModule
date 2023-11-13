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
    public class AchievementService : IAchievementService
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
 
        public IReadOnlyList<Libs.Achievement.AchievementTableEntry> Achievements { get; private set; }

        public IReadOnlyList<Libs.Achievement.CollectionAchievementTable> AchievementDetails { get; private set; }

        public IEnumerable<AchievementGroup> AchievementGroups { get; private set; }

        public IEnumerable<AchievementCategory> AchievementCategories { get; private set; }

        public IReadOnlyList<Libs.Achievement.SubPageInformation> Subpages { get; private set; }


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
                        if (localMetadata.Version != githubMetadata.Version || 
                            !this.CheckMd5(githubMetadata.AchievementDataMd5, Path.Combine(dataFolder, AchievementDataFileName)) ||
                            !this.CheckMd5(githubMetadata.AchievementTablesMd5, Path.Combine(dataFolder, AchievementTablesFileName)) ||
                            !this.CheckMd5(githubMetadata.SubPagesMd5, Path.Combine(dataFolder, SubPagesFileName)))
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

            _ = Task.Run(async () => await this.InitializeApiAchievements());
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


    }
}
