using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services
{
    public class ItemDetailWindowManager : IItemDetailWindowManager, IDisposable
    {
        private readonly Dictionary<string, ItemDetailWindowInformation> windows = new Dictionary<string, ItemDetailWindowInformation>();
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly IPersistanceService persistanceService;
        private readonly IAchievementService achievementService;
        private readonly List<ItemDetailWindowInformation> hiddenWindows = new List<ItemDetailWindowInformation>();

        public ItemDetailWindowManager(IItemDetailWindowFactory itemDetailWindowFactory, IPersistanceService persistanceService, IAchievementService achievementService)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.persistanceService = persistanceService;
            this.achievementService = achievementService;
        }

        public bool ShowWindow(string name)
        {
            if (this.windows.TryGetValue(name, out var window))
            {
                window.Window.Show();
                window.Window.BringWindowToFront();
                return true;
            }

            return false;
        }

        public void Load()
        {
            foreach (var achievement in this.persistanceService.Get().ItemInformation)
            {
                var achievementDetail = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Key);

                foreach (var item in achievement.Value)
                {
                    this.CreateAndShowWindow(item.Value.Name, achievementDetail.ColumnNames, achievementDetail.Entries[item.Value.Index], achievementDetail.Link, item.Value.AchievementId, item.Value.Index);

                    this.windows[item.Value.Name].Window.Location = new Point(item.Value.PositionX, item.Value.PositionY);
                }
            }
        }

        public void CreateAndShowWindow(string name, string[] columns, List<CollectionAchievementTableEntry> item, string achievementLink, int achievementId, int itemIndex)
        {
            if (this.ShowWindow(name))
            {
                return;
            }

            var window = this.itemDetailWindowFactory.Create(name, columns, item, achievementLink);

            window.Parent = GameService.Graphics.SpriteScreen;
            window.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));

            this.windows[name] = new ItemDetailWindowInformation() { Window = window, AchievementId = achievementId, ItemIndex = itemIndex, Name = name };

            _ = this.ShowWindow(name);
        }

        public void Update()
        {
            if (GameService.Gw2Mumble.IsAvailable)
            {
                // TODO: Maybe make this configurable for the case that the user wants to compare a wiki image to the world map?
                if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    foreach (var item in this.windows.Where(x => x.Value.Window.Visible))
                    {
                        if (!this.hiddenWindows.Contains(item.Value))
                        {
                            item.Value.Window.Hide();
                            this.hiddenWindows.Add(item.Value);
                        }
                    }
                }
                else if (this.hiddenWindows.Any())
                {
                    foreach (var item in this.hiddenWindows)
                    {
                        item.Window.Show();
                    }

                    this.hiddenWindows.Clear();
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in this.windows.Where(x => x.Value.Window.Visible))
            {
                this.persistanceService.AddItemInformation(item.Value.AchievementId, item.Value.ItemIndex, item.Value.Name, item.Value.Window.Location.X, item.Value.Window.Location.Y);
                item.Value.Window.Dispose();
            }

            this.windows.Clear();
        }
    }

    public class ItemDetailWindowInformation
    {
        public ItemDetailWindow Window { get; set; }

        public int AchievementId { get; set; }

        public int ItemIndex { get; set; }

        public string Name { get; set; }
    }
}
