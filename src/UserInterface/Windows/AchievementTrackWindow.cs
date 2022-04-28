using Blish_HUD;
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
        private readonly IAchievementDetailsWindowFactory achievementDetailsWindowFactory;
        private readonly Texture2D texture;
        private readonly Dictionary<int, Panel> trackedAchievements = new Dictionary<int, Panel>();
        private readonly Dictionary<int, AchievementDetailsWindow> detachedWindows = new Dictionary<int, AchievementDetailsWindow>();

        private FlowPanel flowPanel;

        public AchievementTrackWindow(ContentsManager contentsManager, IAchievementTrackerService achievementTrackerService, IAchievementControlProvider achievementControlProvider, IAchievementService achievementService, IAchievementDetailsWindowFactory achievementDetailsWindowFactory)
        {
            this.contentsManager = contentsManager;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementControlProvider = achievementControlProvider;
            this.achievementService = achievementService;
            this.achievementDetailsWindowFactory = achievementDetailsWindowFactory;
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

            this.BuildWindow();

            foreach (var item in this.achievementTrackerService.ActiveAchievements)
            {
                this.AchievementTrackerService_AchievementTracked(item);
            }
        }

        private void AchievementTrackerService_AchievementTracked(int achievementId)
        {
            var achievement = this.achievementService.Achievements.First(x => x.Id == achievementId);

            if (this.trackedAchievements.ContainsKey(achievementId))
            {
                return;
            }

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

            detachButton.Click += (s, e) =>
            {
                var trackWindow = this.achievementDetailsWindowFactory.Create(achievement);
                trackWindow.Parent = GameService.Graphics.SpriteScreen;
                trackWindow.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));
                trackWindow.Show();

                this.detachedWindows.Add(achievementId, trackWindow);

                trackWindow.Hidden += (_, eventArgs) =>
                {
                    _ = this.detachedWindows.Remove(achievementId);
                    this.AchievementTrackerService_AchievementTracked(achievementId);
                };

                if (this.trackedAchievements.TryGetValue(achievementId, out var trackedPanel))
                {
                    _ = this.trackedAchievements.Remove(achievementId);
                    trackedPanel.Dispose();
                }
            };

            var control = this.achievementControlProvider.GetAchievementControl(
                achievement,
                achievement.Description,
                new Point(panel.ContentRegion.Width - trackButton.Width - CONTROL_PADDING_LEFT, panel.ContentRegion.Height));

            if (control != null)
            {
                control.Parent = panel;
                control.Height = panel.ContentRegion.Height;
                control.Location = new Point(CONTROL_PADDING_LEFT, CONTROL_PADDING_TOP);
            }

            this.trackedAchievements.Add(achievementId, panel);
        }

        private void BuildWindow()
        {
            // TODO: Localize
            this.Title = "Tracked Achievements";
            this.Emblem = this.contentsManager.GetTexture("605019.png");
            this.ConstructWindow(this.texture, new Rectangle(0, 0, 550, 800), new Rectangle(0, 30, 550, 800 - 30));

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

            foreach (var item in this.detachedWindows)
            {
                item.Value.Dispose();
            }

            this.detachedWindows.Clear();

            this.flowPanel.Dispose();

            base.DisposeControl();
        }

        public override void Draw(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle drawBounds, Microsoft.Xna.Framework.Rectangle scissor)
        {
            if (GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen)
            {
                base.Draw(spriteBatch, drawBounds, scissor);
            }
        }
    }
}
