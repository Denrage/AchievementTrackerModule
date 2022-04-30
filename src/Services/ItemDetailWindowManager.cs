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
        private readonly Dictionary<string, ItemDetailWindow> windows = new Dictionary<string, ItemDetailWindow>();
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private bool purposelyHidden = false;

        public ItemDetailWindowManager(IItemDetailWindowFactory itemDetailWindowFactory)
        {
            this.itemDetailWindowFactory = itemDetailWindowFactory;
        }

        public bool ShowWindow(string name)
        {
            if (this.windows.TryGetValue(name, out var window))
            {
                window.Show();
                window.BringWindowToFront();
                return true;
            }

            return false;
        }

        public void CreateAndShowWindow(string name, string[] columns, List<CollectionAchievementTableEntry> item, string achievementLink)
        {
            if (this.ShowWindow(name))
            {
                return;
            }

            var window = this.itemDetailWindowFactory.Create(name, columns, item, achievementLink);

            window.Parent = GameService.Graphics.SpriteScreen;
            window.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));

            this.windows[name] = window;

            _ = this.ShowWindow(name);
        }

        public void Update()
        {
            if (GameService.Gw2Mumble.IsAvailable)
            {
                // TODO: Maybe make this configurable for the case that the user wants to compare a wiki image to the world map?
                if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    foreach (var item in this.windows)
                    {
                        item.Value.Hide();
                    }

                    this.purposelyHidden = true;
                }
                else if (this.purposelyHidden)
                {
                    foreach (var item in this.windows)
                    {
                        item.Value.Show();
                    }

                    this.purposelyHidden = false;
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in this.windows)
            {
                item.Value.Dispose();
            }

            this.windows.Clear();
        }
    }
}
