using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Windows
{
    public class AchievementTrackWindow : WindowBase2
    {
        private const int CONTROL_PADDING_LEFT = 10;
        private const int CONTROL_PADDING_TOP = 10;

        private readonly ContentsManager contentsManager;
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly IAchievementControlProvider achievementControlProvider;
        private readonly IAchievementService achievementService;
        private readonly IAchievementDetailsWindowManager achievementDetailsWindowManager;
        private readonly IAchievementControlManager achievementControlManager;
        private readonly Texture2D texture;
        private readonly Dictionary<int, Panel> trackedAchievements = new Dictionary<int, Panel>();

        private FlowPanel flowPanel;

        public AchievementTrackWindow(ContentsManager contentsManager, IAchievementTrackerService achievementTrackerService, IAchievementControlProvider achievementControlProvider, IAchievementService achievementService, IAchievementDetailsWindowManager achievementDetailsWindowManager, IAchievementControlManager achievementControlManager)
        {
            this.contentsManager = contentsManager;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementControlProvider = achievementControlProvider;
            this.achievementService = achievementService;
            this.achievementDetailsWindowManager = achievementDetailsWindowManager;
            this.achievementControlManager = achievementControlManager;
            this.texture = this.contentsManager.GetTexture("background.png");
            this.achievementTrackerService.AchievementTracked += this.AchievementTrackerService_AchievementTracked;

            this.achievementTrackerService.AchievementUntracked += achievement =>
            {
                if (this.trackedAchievements.TryGetValue(achievement, out var panel))
                {
                    _ = this.trackedAchievements.Remove(achievement);
                    panel.Dispose();
                }
            };

            this.achievementDetailsWindowManager.WindowHidden += achievementId
                => this.CreatePanel(achievementId);

            this.BuildWindow();

            foreach (var item in this.achievementTrackerService.ActiveAchievements)
            {
                if (!this.achievementDetailsWindowManager.WindowExists(item))
                {
                    this.AchievementTrackerService_AchievementTracked(item);
                }
            }
        }

        private void CreatePanel(int achievementId)
        {
            var achievement = this.achievementService.Achievements.First(x => x.Id == achievementId);

            var panel = new Panel()
            {
                Parent = this.flowPanel,
                CanCollapse = true,
                Title = achievement.Name,
                Width = this.flowPanel.ContentRegion.Width - 16,
                HeightSizingMode = SizingMode.AutoSize,
            };

            var trackButton = new Image()
            {
                Parent = panel,
                Width = 32,
                Height = 32,
                Texture = this.contentsManager.GetTexture("605019.png"),
            };

            trackButton.Location = new Point(panel.ContentRegion.Width - trackButton.Width, 0);

            trackButton.Click += (s, e)
                => this.achievementTrackerService.RemoveAchievement(achievementId);

            var detachButton = new Image()
            {
                Parent = panel,
                Location = new Point(trackButton.Location.X, trackButton.Location.Y + trackButton.Size.Y),
                Width = 32,
                Height = 32,
                Texture = this.contentsManager.GetTexture("pop_out.png"),
            };

            if (achievement.HasLink)
            {
                var wikiButton = new Image()
                {
                    Parent = panel,
                    Location = new Point(detachButton.Location.X, detachButton.Location.Y + detachButton.Size.Y),
                    Width = 32,
                    Height = 32,
                    Texture = this.contentsManager.GetTexture("wiki.png"),
                };

                wikiButton.Click += (s, e)
                    => _ = System.Diagnostics.Process.Start("https://wiki.guildwars2.com" + achievement.Link);
            }

            if (!this.achievementControlManager.ControlExists(achievementId))
            {
                this.achievementControlManager.InitializeControl(achievementId, achievement, achievement.Description);
            }

            var controlPanel = new Panel()
            {
                Parent = panel,
                HeightSizingMode = SizingMode.AutoSize,
                Width = panel.ContentRegion.Width - trackButton.Width - CONTROL_PADDING_LEFT,
                Location = new Point(CONTROL_PADDING_LEFT, CONTROL_PADDING_TOP),
            };

            this.achievementControlManager.ChangeParent(achievementId, controlPanel);

            detachButton.Click += (s, e) =>
            {
                this.achievementDetailsWindowManager.CreateWindow(achievement);

                if (this.trackedAchievements.TryGetValue(achievementId, out var trackedPanel))
                {
                    _ = this.trackedAchievements.Remove(achievementId);
                    trackedPanel.Dispose();
                }
            };

            this.trackedAchievements.Add(achievementId, panel);
        }

        private void AchievementTrackerService_AchievementTracked(int achievementId)
        {
            var achievement = this.achievementService.Achievements.First(x => x.Id == achievementId);

            if (this.trackedAchievements.ContainsKey(achievementId))
            {
                return;
            }

            this.CreatePanel(achievementId);
        }

        private void BuildWindow()
        {
            // TODO: Localize
            this.Title = "Tracked";
            this.Emblem = this.contentsManager.GetTexture("605019.png");
            this.ConstructWindow(this.texture, new Rectangle(0, 0, 350, 600), new Rectangle(0, 30, 350, 600 - 30));

            this.flowPanel = new FlowPanel()
            {
                Parent = this,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Size = this.ContentRegion.Size,
                ControlPadding = new Vector2(7f),
            };
        }

        protected override void DisposeControl()
        {
            foreach (var item in this.trackedAchievements)
            {
                item.Value.Dispose();
            }

            this.trackedAchievements.Clear();

            this.flowPanel.Dispose();

            base.DisposeControl();
        }
    }
}
