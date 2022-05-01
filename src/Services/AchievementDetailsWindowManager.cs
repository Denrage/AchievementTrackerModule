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
        private readonly IAchievementControlManager achievementControlManager;
        private readonly IPersistanceService persistanceService;
        private readonly IAchievementService achievementService;
        private readonly List<AchievementDetailsWindow> hiddenWindows = new List<AchievementDetailsWindow>();

        public event Action<int> WindowHidden;

        public AchievementDetailsWindowManager(IAchievementDetailsWindowFactory achievementDetailsWindowFactory, IAchievementControlManager achievementControlManager, IPersistanceService persistanceService, IAchievementService achievementService)
        {
            this.achievementDetailsWindowFactory = achievementDetailsWindowFactory;
            this.achievementControlManager = achievementControlManager;
            this.persistanceService = persistanceService;
            this.achievementService = achievementService;
        }

        public void Load()
        {
            foreach (var item in this.persistanceService.Get().AchievementInformation)
            {
                var achievement = this.achievementService.Achievements.FirstOrDefault(x => x.Id == item.Key);
                this.CreateWindow(achievement);

                this.windows[item.Key].Location = new Point(item.Value.PositionX, item.Value.PositionY);
            }
        }

        public void CreateWindow(AchievementTableEntry achievement)
        {
            var window = this.achievementDetailsWindowFactory.Create(achievement);

            window.Parent = GameService.Graphics.SpriteScreen;
            window.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));

            window.Hidden += (s, e) =>
            {
                if (!this.hiddenWindows.Any())
                {
                    _ = this.windows.Remove(achievement.Id);
                    this.achievementControlManager.RemoveParent(achievement.Id);
                    window.Dispose();
                    this.WindowHidden?.Invoke(achievement.Id);
                }
            };

            this.windows[achievement.Id] = window;

            window.Show();
        }

        public bool WindowExists(int achievementId)
            => this.windows.ContainsKey(achievementId);

        public void Update()
        {
            if (GameService.Gw2Mumble.IsAvailable)
            {
                if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    foreach (var item in this.windows)
                    {
                        this.hiddenWindows.Add(item.Value);
                        item.Value.Hide();
                    }
                }
                else if (this.hiddenWindows.Any())
                {
                    foreach (var item in this.hiddenWindows)
                    {
                        item.Show();
                    }

                    this.hiddenWindows.Clear();
                }
            }
        }

        public void Dispose()
        {
            foreach (var item in this.windows.Where(x => x.Value.Visible))
            {
                this.persistanceService.AddAchievementWindowInformation(item.Key, item.Value.Location.X, item.Value.Location.Y);
                item.Value.Dispose();
            }

            this.windows.Clear();
        }
    }
}
