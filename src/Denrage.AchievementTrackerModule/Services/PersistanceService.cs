using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class PersistanceService : IPersistanceService
    {
        private const string SAVE_FILE_NAME = "persistanceStorage.json";
        private readonly DirectoriesManager directoriesManager;
        private readonly ItemDetailWindowManager itemDetailWindowManager;
        private readonly AchievementTrackerService achievementTrackerService;
        private readonly Logger logger;
        private readonly ISettingsService settings;
        private readonly List<Action<Storage>> registeredSaveDelegates = new List<Action<Storage>>();
        private Storage storage;
        private Task autoSaveTask;
        private CancellationTokenSource autoSaveCancellationTokenSource;

        public event Action AutoSave;

        public PersistanceService(
            DirectoriesManager directoriesManager,
            ItemDetailWindowManager itemDetailWindowManager,
            AchievementTrackerService achievementTrackerService,
            Logger logger,
            ISettingsService settings)
        {
            this.directoriesManager = directoriesManager;
            this.itemDetailWindowManager = itemDetailWindowManager;
            this.achievementTrackerService = achievementTrackerService;
            this.logger = logger;
            this.settings = settings;
            this.settings.AutoSave.SettingChanged += (s, e) =>
            {
                if (e.NewValue)
                {
                    this.InitializeAutoSaveTask();
                }
                else
                {
                    this.ResetAutoSaveTask();
                }
            };

            if (this.settings.AutoSave.Value)
            {
                this.InitializeAutoSaveTask();
            }
        }

        private void ResetAutoSaveTask()
        {
            if (this.autoSaveTask != null)
            {
                this.autoSaveCancellationTokenSource.Cancel();
                this.autoSaveTask = null;
            }
        }

        private void InitializeAutoSaveTask()
        {
            this.ResetAutoSaveTask();

            this.autoSaveCancellationTokenSource = new CancellationTokenSource();
            this.autoSaveTask = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5), this.autoSaveCancellationTokenSource.Token);
                        this.AutoSave?.Invoke();
                    }
                }
                catch (TaskCanceledException)
                {
                }
            }, this.autoSaveCancellationTokenSource.Token);

        }

        public void RegisterSaveDelegate(Action<Storage> action)
        {
            this.registeredSaveDelegates.Add(action);
        }

        public void Save(int achievementTrackWindowLocationX, int achievementTrackWindowLocationY, bool showTrackWindow)
        {
            var storage = new Storage();

            foreach (var item in this.itemDetailWindowManager.Windows.Where(x => x.Value.Window.Visible))
            {
                if (!storage.ItemInformation.TryGetValue(item.Value.AchievementId, out var itemWindows))
                {
                    itemWindows = new Dictionary<int, ItemInformation>();
                    storage.ItemInformation[item.Value.AchievementId] = itemWindows;
                }

                itemWindows[item.Value.ItemIndex] = new ItemInformation()
                {
                    AchievementId = item.Value.AchievementId,
                    Index = item.Value.ItemIndex,
                    Name = item.Value.Name,
                    PositionX = item.Value.Window.Location.X,
                    PositionY = item.Value.Window.Location.Y,
                };
            }

            foreach (var item in this.registeredSaveDelegates)
            {
                item(storage);
            }

            storage.TrackedAchievements.AddRange(this.achievementTrackerService.ActiveAchievements);

            storage.TrackWindowLocationX = achievementTrackWindowLocationX;
            storage.TrackWindowLocationY = achievementTrackWindowLocationY;
            storage.ShowTrackWindow = showTrackWindow;

            try
            {
                var safeFolder = this.directoriesManager.GetFullDirectoryPath("achievement_module");

                _ = System.IO.Directory.CreateDirectory(safeFolder);

                System.IO.File.WriteAllText(System.IO.Path.Combine(safeFolder, SAVE_FILE_NAME), System.Text.Json.JsonSerializer.Serialize(storage));
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on saving persistent information");
            }
        }

        public Storage Get()
        {
            try
            {
                var safeFolder = this.directoriesManager.GetFullDirectoryPath("achievement_module");
                var file = System.IO.Path.Combine(safeFolder, SAVE_FILE_NAME);
                if (this.storage is null)
                {
                    this.storage = System.IO.File.Exists(file)
                        ? System.Text.Json.JsonSerializer.Deserialize<Storage>(System.IO.File.ReadAllText(file))
                        : new Storage();
                }

                return this.storage;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on reading persistent information");
                return new Storage();
            }
        }
    }
}
