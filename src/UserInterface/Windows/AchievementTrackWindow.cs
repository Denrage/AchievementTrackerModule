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
        private readonly Dictionary<Gw2Sharp.WebApi.V2.Models.Achievement, Panel> trackedAchievements = new Dictionary<Gw2Sharp.WebApi.V2.Models.Achievement, Panel>();
        private readonly Dictionary<Gw2Sharp.WebApi.V2.Models.Achievement, AchievementDetailsWindow> detachedWindows = new Dictionary<Gw2Sharp.WebApi.V2.Models.Achievement, AchievementDetailsWindow>();
        
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

        private void AchievementTrackerService_AchievementTracked(Gw2Sharp.WebApi.V2.Models.Achievement achievement)
        {
            if (this.trackedAchievements.ContainsKey(achievement))
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
                BackgroundColor = Microsoft.Xna.Framework.Color.White,
            };

            trackButton.Location = new Microsoft.Xna.Framework.Point(panel.ContentRegion.Width - trackButton.Width, 0);

            trackButton.Click += (s, e) 
                => this.achievementTrackerService.RemoveAchievement(achievement);

            var detachButton = new Image()
            {
                Parent = panel,
                Location = new Microsoft.Xna.Framework.Point(trackButton.Location.X, trackButton.Location.Y + trackButton.Size.Y),
                Width = 32,
                Height = 32,
                Texture = this.contentsManager.GetTexture("pop_out.png"),
                BackgroundColor = Microsoft.Xna.Framework.Color.White,
            };

            detachButton.Click += (s, e) =>
            {
                var trackWindow = this.achievementDetailsWindowFactory.Create(achievement);
                trackWindow.Parent = GameService.Graphics.SpriteScreen;
                trackWindow.Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2));
                trackWindow.Show();

                this.detachedWindows.Add(achievement, trackWindow);

                trackWindow.Hidden += (_, eventArgs) =>
                {
                    _ = this.detachedWindows.Remove(achievement);
                    this.AchievementTrackerService_AchievementTracked(achievement);
                };

                if (this.trackedAchievements.TryGetValue(achievement, out var trackedPanel))
                {
                    _ = this.trackedAchievements.Remove(achievement);
                    trackedPanel.Dispose();
                }
            };

            var control = this.achievementControlProvider.GetAchievementControl(
                achievement, 
                this.achievementService.Achievements.FirstOrDefault(x => x.Id == achievement.Id).Description,
                new Microsoft.Xna.Framework.Point(panel.ContentRegion.Width - trackButton.Width - CONTROL_PADDING_LEFT, panel.ContentRegion.Height));

            if (control != null)
            {
                control.Parent = panel;
                control.Height = panel.ContentRegion.Height;
                control.Location = new Point(CONTROL_PADDING_LEFT, CONTROL_PADDING_TOP);
            }

            this.trackedAchievements.Add(achievement, panel);
        }

        private void BuildWindow()
        {
            // TODO: Localize
            this.Title = "Tracked Achievements";
            this.Emblem = this.contentsManager.GetTexture("605019.png");
            this.ConstructWindow(this.texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 550, 800), new Microsoft.Xna.Framework.Rectangle(0, 30, 550, 800 - 30));

            this.flowPanel = new FlowPanel()
            {
                Parent = this,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Size = this.ContentRegion.Size,
                
            };

        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds)
        {
            //spriteBatch.DrawOnCtrl(this,
            //                       this.texture,
            //                       bounds);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
