using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class PersistanceService : IPersistanceService
    {
        private const string SAVE_FILE_NAME = "persistanceStorage.json";
        private Storage storage;
        private readonly DirectoriesManager directoriesManager;

        public PersistanceService(DirectoriesManager directoriesManager)
        {
            this.directoriesManager = directoriesManager;
        }

        public void AddAchievementWindowInformation(int achievementId, int positionX, int positionY)
        {
            if (this.storage is null)
            {
                this.storage = new Storage();
            }

            this.storage.AchievementInformation[achievementId] = new AchievementWindowInformation()
            {
                AchievementId = achievementId,
                PositionX = positionX,
                PositionY = positionY,
            };
        }

        public void AddItemInformation(int achievementId, int itemIndex, string name, int positionX, int positionY)
        {
            if (this.storage is null)
            {
                this.storage = new Storage();
            }

            if (!this.storage.ItemInformation.TryGetValue(achievementId, out var itemInformation))
            {
                itemInformation = new Dictionary<int, ItemInformation>();
                this.storage.ItemInformation[achievementId] = itemInformation;
            }

            itemInformation[itemIndex] = new ItemInformation()
            {
                Name = name,
                AchievementId = achievementId,
                Index = itemIndex,
                PositionX = positionX,
                PositionY = positionY,
            };
        }

        public void TrackAchievement(int achievementId)
        {
            if (this.storage is null)
            {
                this.storage = new Storage();
            }

            this.storage.TrackedAchievements.Add(achievementId);
        }

        public void Save()
        {
            var safeFolder = this.directoriesManager.GetFullDirectoryPath("achievement_module");

            _ = System.IO.Directory.CreateDirectory(safeFolder);

            System.IO.File.WriteAllText(System.IO.Path.Combine(safeFolder, SAVE_FILE_NAME), System.Text.Json.JsonSerializer.Serialize(this.storage));
        }

        public Storage Get()
        {

            var safeFolder = this.directoriesManager.GetFullDirectoryPath("achievement_module");
            var file = System.IO.Path.Combine(safeFolder, SAVE_FILE_NAME);
            if (!System.IO.File.Exists(file))
            {
                return new Storage();
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<Storage>(System.IO.File.ReadAllText(file));
            return result;
        }
    }

    public class Storage
    {
        public Dictionary<int, AchievementWindowInformation> AchievementInformation { get; set; } = new Dictionary<int, AchievementWindowInformation>();

        public Dictionary<int, Dictionary<int, ItemInformation>> ItemInformation { get; set; } = new Dictionary<int, Dictionary<int, ItemInformation>>();

        public List<int> TrackedAchievements { get; set; } = new List<int>();
    }

    public class AchievementWindowInformation
    {
        public int AchievementId { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }
    }

    public class ItemInformation
    {
        public int AchievementId { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }
    }
}
