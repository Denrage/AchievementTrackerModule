using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AchievementTrackerModule.Services;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        // TODO: Logging
        private static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly DependencyInjectionContainer dependencyInjectionContainer;
        private readonly List<AchievementTrackWindow> windows;

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
            this.dependencyInjectionContainer = new DependencyInjectionContainer(this.Gw2ApiManager, this.ContentsManager);
            this.windows = new List<AchievementTrackWindow>();
        }

        protected override void DefineSettings(SettingCollection settings)
        {
        }

        protected override void Initialize()
        {
            this.Gw2ApiManager.SubtokenUpdated += async (_, args)
                => await this.dependencyInjectionContainer.AchievementService.LoadPlayerAchievements();
        }

        protected override async Task LoadAsync()
        {
            await this.dependencyInjectionContainer.InitializeAsync();

            this.dependencyInjectionContainer.AchievementTrackerService.AchievementTracked += this.AchievementTrackerService_AchievementTracked;

            _ = GameService.Overlay.BlishHudWindow.AddTab(
                "AchievementTracker",
                this.ContentsManager.GetTexture("243.png"),
                () => new AchievementTrackerView(
                    this.dependencyInjectionContainer.AchievementApiService,
                    this.dependencyInjectionContainer.AchievementCategoryOverviewFactory));

            await base.LoadAsync();
        }

        private void AchievementTrackerService_AchievementTracked(Achievement achievement)
        {
            var trackWindow = new AchievementTrackWindow(
                this.ContentsManager,
                achievement,
                this.dependencyInjectionContainer.AchievementService,
                this.dependencyInjectionContainer.AchievementControlProvider)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2)),
            };

            trackWindow.ToggleWindow();

            this.windows.Add(trackWindow);
        }

        protected override void OnModuleLoaded(EventArgs e) =>
            base.OnModuleLoaded(e);

        protected override void Update(GameTime gameTime)
        {
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }
    }
}
