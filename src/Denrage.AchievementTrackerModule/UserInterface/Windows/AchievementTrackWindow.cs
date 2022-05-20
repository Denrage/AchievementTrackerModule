using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        private readonly OverlayService overlayService;
        private readonly Func<IView> achievementOverviewView;
        private readonly Texture2D texture;
        private readonly Dictionary<int, Panel> trackedAchievements = new Dictionary<int, Panel>();

        private FlowPanel flowPanel;
        private Label noAchievementsLabel;

        public AchievementTrackWindow(ContentsManager contentsManager, IAchievementTrackerService achievementTrackerService, IAchievementControlProvider achievementControlProvider, IAchievementService achievementService, IAchievementDetailsWindowManager achievementDetailsWindowManager, IAchievementControlManager achievementControlManager, OverlayService overlayService, Func<IView> achievementOverviewView)
        {
            this.contentsManager = contentsManager;
            this.achievementTrackerService = achievementTrackerService;
            this.achievementControlProvider = achievementControlProvider;
            this.achievementService = achievementService;
            this.achievementDetailsWindowManager = achievementDetailsWindowManager;
            this.achievementControlManager = achievementControlManager;
            this.overlayService = overlayService;
            this.achievementOverviewView = achievementOverviewView;
            this.texture = this.contentsManager.GetTexture("background.png");
            this.achievementTrackerService.AchievementTracked += this.AchievementTrackerService_AchievementTracked;

            this.achievementTrackerService.AchievementUntracked += achievement =>
            {
                if (this.trackedAchievements.TryGetValue(achievement, out var panel))
                {
                    _ = this.trackedAchievements.Remove(achievement);
                    this.achievementControlManager.RemoveParent(achievement);
                    panel.Dispose();
                }

                if (this.trackedAchievements.Count == 0)
                {
                    this.noAchievementsLabel.Visible = true;
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

        public class TestPanel : Panel
        {

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
                    Texture = this.contentsManager.GetTexture("link.png"),
                };

                wikiButton.Click += (s, e)
                    =>
                {
                    var inSubpages = false;
                    foreach (var subPage in this.achievementService.Subpages)
                    {
                        if (subPage.Link.Contains(achievement.Link))
                        {
                            inSubpages = true;
                            var window = new SubPageInformationWindow(this.contentsManager, this.achievementService, subPage)
                            {
                                Parent = GameService.Graphics.SpriteScreen,
                            };

                            window.Hidden += (sender, _) => window.Dispose();
                            window.Show();
                        }
                    }

                    if (!inSubpages)
                    {
                        _ = System.Diagnostics.Process.Start("https://wiki.guildwars2.com" + achievement.Link);
                    }
                };
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
            if (this.noAchievementsLabel.Visible)
            {
                this.noAchievementsLabel.Visible = false;
            }

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

            var openAchievementPanelButton = new StandardButton()
            {
                // TODO: Localize
                Text = "Open Achievement Panel",
                Height = 30,
                Width = this.ContentRegion.Width,
                Parent = this,
            };

            this.flowPanel = new FlowPanel()
            {
                Parent = this,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Width = this.ContentRegion.Width,
                Height = this.ContentRegion.Height - openAchievementPanelButton.Height,
                ControlPadding = new Vector2(7f),
            };

            openAchievementPanelButton.Location = new Point(0, this.flowPanel.Height);

            openAchievementPanelButton.Click += (s, e) =>
            {
                if (!this.overlayService.BlishHudWindow.Visible)
                {
                    this.overlayService.BlishHudWindow.Show();
                }

                this.overlayService.BlishHudWindow.Navigate(this.achievementOverviewView());
            };

            this.noAchievementsLabel = new Label()
            {
                Parent = this.flowPanel,
                Height = this.flowPanel.ContentRegion.Height,
                Width = this.flowPanel.ContentRegion.Width,
                // TODO: Localize
                Text = "You currently don't track any achievements.\n To open the achievement overview, either press\n the button below or open the\n blishhud window and navigate to the achievement tab.",
                Visible = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
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
