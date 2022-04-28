using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementDetailsWindowManager : IAchievementDetailsWindowManager
    {
        private readonly Dictionary<int, AchievementDetailsWindow> windows = new Dictionary<int, AchievementDetailsWindow>();
        private readonly IAchievementDetailsWindowFactory achievementDetailsWindowFactory;
        private bool purposelyHidden = false;

        public event Action<int> WindowHidden;

        public AchievementDetailsWindowManager(IAchievementDetailsWindowFactory achievementDetailsWindowFactory)
        {
            this.achievementDetailsWindowFactory = achievementDetailsWindowFactory;
        }

        public void CreateWindow(AchievementTableEntry achievement)
        {
            var window = this.achievementDetailsWindowFactory.Create(achievement);

            window.Parent = GameService.Graphics.SpriteScreen;
            window.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));

            window.Hidden += (s, e) =>
            {
                if (!this.purposelyHidden)
                {
                    _ = this.windows.Remove(achievement.Id);
                    window.Dispose();
                    this.WindowHidden?.Invoke(achievement.Id);
                }
            };

            this.windows[achievement.Id] = window;

            window.Show();
        }

        public void Update()
        {
            // TODO: Maybe make this configurable for the case that the user wants to compare a wiki image to the world map?
            if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
            {
                this.purposelyHidden = true;
                foreach (var item in this.windows)
                {
                    if (item.Value.Visible)
                    {
                        item.Value.Hide();
                    }
                }
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
