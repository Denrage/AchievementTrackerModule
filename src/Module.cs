using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2.Models;
using System.Text.Json.Serialization;
using Gw2Sharp.WebApi.V2.Clients;
using System.Collections.Generic;

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
        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {

            achievementTrackerService = new AchievementTrackerService();
            achievementApiService = new AchievementApiService(Gw2ApiManager);
            achievementListItemFactory = new AchievementListItemFactory(achievementTrackerService);
            achievementCategoryOverviewFactory = new AchievementCategoryOverviewFactory(Gw2ApiManager, achievementListItemFactory);
            achievementService = new AchievementService(this.ContentsManager);
            windows = new List<AchievementTrackWindow>();
        }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {
            Gw2ApiManager.SubtokenUpdated += (sender, args) =>
            {

            };
        }

        protected override async Task LoadAsync()
        {
            await achievementApiService.LoadAsync();

            await this.achievementService.LoadAsync();

            achievementTrackerService.AchievementTracked += AchievementTrackerService_AchievementTracked;
            GameService.Overlay.BlishHudWindow.AddTab("AchievementTracker", ContentsManager.GetTexture("243.png"), () => new AchievementTrackerView(achievementApiService, achievementCategoryOverviewFactory));
        }

        private void AchievementTrackerService_AchievementTracked(Achievement achievement)
        {
            var trackWindow = new AchievementTrackWindow(ContentsManager, achievement, Gw2ApiManager, this.achievementService)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2),
            };

            trackWindow.ToggleWindow();

            windows.Add(trackWindow);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

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
