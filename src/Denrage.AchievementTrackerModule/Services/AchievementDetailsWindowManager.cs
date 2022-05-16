using Blish_HUD;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementDetailsWindowManager : IAchievementDetailsWindowManager
    {
        private readonly IAchievementDetailsWindowFactory achievementDetailsWindowFactory;
        private readonly IAchievementControlManager achievementControlManager;
        private readonly IAchievementService achievementService;
        private readonly Logger logger;
        private readonly List<AchievementDetailsWindow> hiddenWindows = new List<AchievementDetailsWindow>();

        internal Dictionary<int, AchievementDetailsWindow> Windows { get; } = new Dictionary<int, AchievementDetailsWindow>();

        public event Action<int> WindowHidden;

        public AchievementDetailsWindowManager(
            IAchievementDetailsWindowFactory achievementDetailsWindowFactory,
            IAchievementControlManager achievementControlManager,
            IAchievementService achievementService,
            Logger logger)
        {
            this.achievementDetailsWindowFactory = achievementDetailsWindowFactory;
            this.achievementControlManager = achievementControlManager;
            this.achievementService = achievementService;
            this.logger = logger;
        }

        public void Load(IPersistanceService persistanceService)
        {
            try
            {
                foreach (var item in persistanceService.Get().AchievementInformation)
                {
                    var achievement = this.achievementService.Achievements.FirstOrDefault(x => x.Id == item.Key);
                    this.CreateWindow(achievement);

                    this.Windows[item.Key].Location = new Point(item.Value.PositionX, item.Value.PositionY);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Exception occured on restoring window positions");
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
                    _ = this.Windows.Remove(achievement.Id);
                    this.achievementControlManager.RemoveParent(achievement.Id);
                    window.Dispose();
                    this.WindowHidden?.Invoke(achievement.Id);
                }
            };

            this.Windows[achievement.Id] = window;

            if (GameService.Gw2Mumble.IsAvailable && (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen) && !this.hiddenWindows.Contains(window))
            {
                this.hiddenWindows.Add(window);
            }
            else
            {
                window.Show();
            }
        }

        public bool WindowExists(int achievementId)
            => this.Windows.ContainsKey(achievementId);

        public void Update()
        {
            if (GameService.Gw2Mumble.IsAvailable)
            {
                if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    foreach (var item in this.Windows)
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
            foreach (var item in this.Windows)
            {
                item.Value.Dispose();
            }

            this.Windows.Clear();
        }
    }
}
