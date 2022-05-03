using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services
{
    public class ItemDetailWindowManager : IItemDetailWindowManager, IDisposable
    {
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly IAchievementService achievementService;
        private readonly Logger logger;
        private readonly List<ItemDetailWindowInformation> hiddenWindows = new List<ItemDetailWindowInformation>();

        internal Dictionary<string, ItemDetailWindowInformation> Windows { get; } = new Dictionary<string, ItemDetailWindowInformation>();

        public ItemDetailWindowManager(IItemDetailWindowFactory itemDetailWindowFactory, IAchievementService achievementService, Logger logger)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.achievementService = achievementService;
            this.logger = logger;
        }

        public bool ShowWindow(string name)
        {
            if (this.Windows.TryGetValue(name, out var window))
            {
                if (GameService.Gw2Mumble.IsAvailable && (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen) && !this.hiddenWindows.Contains(window))
                {
                    this.hiddenWindows.Add(window);
                }
                else
                {
                    window.Window.Show();
                    window.Window.BringWindowToFront();
                }

                return true;
            }

            return false;
        }

        public void Load(IPersistanceService persistanceService)
        {
            try
            {
                foreach (var achievement in persistanceService.Get().ItemInformation)
                {
                    var achievementDetail = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Key);

                    foreach (var item in achievement.Value)
                    {
                        this.CreateAndShowWindow(item.Value.Name, achievementDetail.ColumnNames, achievementDetail.Entries[item.Value.Index], achievementDetail.Link, item.Value.AchievementId, item.Value.Index);

                        this.Windows[item.Value.Name].Window.Location = new Point(item.Value.PositionX, item.Value.PositionY);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on restoring window positions");
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

            this.Windows[name] = new ItemDetailWindowInformation() { Window = window, AchievementId = achievementId, ItemIndex = itemIndex, Name = name };

            _ = this.ShowWindow(name);
        }

        public void Update()
        {
            if (GameService.Gw2Mumble.IsAvailable)
            {
                // TODO: Maybe make this configurable for the case that the user wants to compare a wiki image to the world map?
                if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    foreach (var item in this.Windows.Where(x => x.Value.Window.Visible))
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
            foreach (var item in this.Windows)
            {
                item.Value.Window.Dispose();
            }

            this.Windows.Clear();
        }
    }
}
