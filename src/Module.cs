using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
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
        private static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly IAchievementTrackerService achievementTrackerService;
        private readonly AchievementApiService achievementApiService;
        private readonly IAchievementListItemFactory achievementListItemFactory;
        private readonly IAchievementCategoryOverviewFactory achievementCategoryOverviewFactory;
        private readonly AchievementService achievementService;
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

            this.achievementTrackerService = new AchievementTrackerService();
            this.achievementApiService = new AchievementApiService(this.Gw2ApiManager);
            this.achievementListItemFactory = new AchievementListItemFactory(this.achievementTrackerService);
            this.achievementCategoryOverviewFactory = new AchievementCategoryOverviewFactory(this.Gw2ApiManager, this.achievementListItemFactory);
            this.achievementService = new AchievementService(this.ContentsManager, this.Gw2ApiManager);
            this.windows = new List<AchievementTrackWindow>();
        }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {
            this.Gw2ApiManager.SubtokenUpdated += (sender, args) =>
            {

            };
        }

        protected override async Task LoadAsync()
        {
            await this.achievementApiService.LoadAsync();

            await this.achievementService.LoadAsync();

            this.achievementTrackerService.AchievementTracked += this.AchievementTrackerService_AchievementTracked;
            GameService.Overlay.BlishHudWindow.AddTab("AchievementTracker", this.ContentsManager.GetTexture("243.png"), () => new AchievementTrackerView(this.achievementApiService, this.achievementCategoryOverviewFactory));
        }

        private void AchievementTrackerService_AchievementTracked(Achievement achievement)
        {
            var trackWindow = new AchievementTrackWindow(this.ContentsManager, achievement, this.achievementService, new AchievementTrackWindow.AchievementControlProvider(this.achievementService, new ItemDetailWindowFactory(this.ContentsManager, this.achievementService, new AchievementTableEntryProvider(this.achievementService))))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2),
            };

            trackWindow.ToggleWindow();

            this.windows.Add(trackWindow);
        }

        protected override void OnModuleLoaded(EventArgs e) =>

            // Base handler must be called
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
