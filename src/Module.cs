using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AchievementTrackerModule.Services;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    // TODO: Save tracked achievements
    // TODO: Reduce size of windows
    // TODO: Fix window hide on map open
    // TODO: Logging
    // TODO: Handle Exceptions / Failed API-Requests
    // TODO: Wiki Link button like gw2efficiecny on achievement details/trackwindow
    // TODO: Use Microsoft.Extensions.DependencyInjection
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly DependencyInjectionContainer dependencyInjectionContainer;
        private AchievementTrackWindow window;
        private CornerIcon cornerIcon;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            this.dependencyInjectionContainer = new DependencyInjectionContainer(this.Gw2ApiManager, this.ContentsManager, GameService.Content);
        }

        protected override void DefineSettings(SettingCollection settings)
        {
        }

        protected override void Initialize()
        {
            this.Gw2ApiManager.SubtokenUpdated += async (_, args)
                => await this.dependencyInjectionContainer.AchievementService.LoadPlayerAchievements();

            this.cornerIcon = new CornerIcon()
            {
                // TODO: Localize
                IconName = "Open Achievement Panel",
                Icon = this.ContentsManager.GetTexture(@"corner_icon_inactive.png"),
                HoverIcon = this.ContentsManager.GetTexture(@"corner_icon_active.png"),
                Width = 64,
                Height = 64,
            };

            this.cornerIcon.Click += (s, e) =>
            {
                this.InitializeWindow();

                this.window.ToggleWindow();
            };
        }

        protected override async Task LoadAsync()
        {
            await this.dependencyInjectionContainer.InitializeAsync();

            this.dependencyInjectionContainer.AchievementTrackerService.AchievementTracked += this.AchievementTrackerService_AchievementTracked;

            _ = GameService.Overlay.BlishHudWindow.AddTab(
                "AchievementTracker",
                this.ContentsManager.GetTexture("achievement_icon.png"),
                () => new AchievementTrackerView(
                    this.dependencyInjectionContainer.AchievementItemOverviewFactory,
                    this.dependencyInjectionContainer.AchievementService));

            await base.LoadAsync();
        }

        private void InitializeWindow()
        {
            if (this.window is null)
            {
                this.window = new AchievementTrackWindow(this.ContentsManager, this.dependencyInjectionContainer.AchievementTrackerService, this.dependencyInjectionContainer.AchievementControlProvider, this.dependencyInjectionContainer.AchievementService, this.dependencyInjectionContainer.AchievementDetailsWindowManager, this.dependencyInjectionContainer.AchievementControlManager)
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2)),
                };
            }
        }

        private void AchievementTrackerService_AchievementTracked(int achievement)
        {
            this.InitializeWindow();

            if (!this.window.Visible)
            {
                this.window.Show();
            }
        }

        protected override void OnModuleLoaded(EventArgs e) =>
            base.OnModuleLoaded(e);

        protected override void Update(GameTime gameTime)
        {
            this.dependencyInjectionContainer.ItemDetailWindowManager.Update();
            this.dependencyInjectionContainer.AchievementDetailsWindowManager.Update();
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            this.cornerIcon.Dispose();
            this.window?.Dispose();
        }
    }
}
