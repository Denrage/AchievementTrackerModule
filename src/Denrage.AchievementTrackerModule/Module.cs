using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AchievementTrackerModule.Services;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    // TODO: Use Microsoft.Extensions.DependencyInjection
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly DependencyInjectionContainer dependencyInjectionContainer;
        private readonly Logger logger;
        private Func<IView> achievementOverviewView;
        private AchievementTrackWindow window;
        private CornerIcon cornerIcon;
        private bool purposelyHidden;
        private SettingEntry<bool> autoSave;
        private SettingEntry<bool> limitAchievements;
        private SettingEntry<bool> bitAlignmentValidation;
        private WindowTab blishhudOverlayTab;

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
            this.logger = Logger;
            this.dependencyInjectionContainer = new DependencyInjectionContainer(this.Gw2ApiManager, this.ContentsManager, GameService.Content, this.DirectoriesManager, this.logger, GameService.Graphics);
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            this.autoSave = settings.DefineSetting("AutoSave", false, () => "Auto save every 5 minutes", () => "Auto save tracked achievements, windows and their positions every 5 minutes");

            this.limitAchievements = settings.DefineSetting("LimitAchievements", true, () => "Limit Achievements to 15", () => "This will limit the maximum of achievements to 15. If it's disabled expect performance and usability issues.");

            this.bitAlignmentValidation = settings.DefineSetting("BitAlignmentValidation", false, () => "Validate bit alignment (debug)", () => "One-time startup check: aligns every collection/objective achievement and logs a summary, including a comparison against the old hand-written table. Leave off unless debugging \"what's left\" text.");
        }

        protected override void Initialize()
        {
            this.Gw2ApiManager.SubtokenUpdated += async (_, args) =>
            {
                this.logger.Info("Subtoken updated");
                if (this.dependencyInjectionContainer != null && this.dependencyInjectionContainer.AchievementService != null)
                {
                    await this.dependencyInjectionContainer?.AchievementService?.LoadPlayerAchievements();
                }
            };
        }

        protected override async Task LoadAsync()
        {
            _ = Task.Run(async () =>
            {

                this.achievementOverviewView = () => new AchievementTrackerView(
                        this.dependencyInjectionContainer.AchievementItemOverviewFactory,
                        this.dependencyInjectionContainer.AchievementService,
                        this.dependencyInjectionContainer.TextureService);

                await Task.Delay(TimeSpan.FromSeconds(3));
                await this.dependencyInjectionContainer.InitializeAsync(this.autoSave, this.limitAchievements);
                this.dependencyInjectionContainer.AchievementTrackerService.AchievementTracked += this.AchievementTrackerService_AchievementTracked;
                this.dependencyInjectionContainer.AchievementTrackerService.AchievementTracked += this.PrefetchBitAlignment;

                if (this.bitAlignmentValidation.Value)
                {
                    _ = this.dependencyInjectionContainer.BitAlignmentService.RunValidationAsync();
                }

                if (this.dependencyInjectionContainer.PersistanceService.Get().ShowTrackWindow)
                {
                    this.InitializeWindow();
                    this.window.Show();
                }

                this.blishhudOverlayTab = GameService.Overlay.BlishHudWindow.AddTab(
                    "Achievement Tracker",
                    this.ContentsManager.GetTexture("achievement_icon.png"),
                    this.achievementOverviewView);

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

                this.dependencyInjectionContainer.PersistanceService.AutoSave += this.SavePersistentInformation;
            });

            await base.LoadAsync();
        }

        private void InitializeWindow()
        {
            if (this.window is null)
            {
                this.window = new AchievementTrackWindow(
                    this.ContentsManager,
                    this.dependencyInjectionContainer.AchievementTrackerService,
                    this.dependencyInjectionContainer.AchievementControlProvider,
                    this.dependencyInjectionContainer.AchievementService,
                    this.dependencyInjectionContainer.AchievementDetailsWindowManager,
                    this.dependencyInjectionContainer.AchievementControlManager,
                    this.dependencyInjectionContainer.SubPageInformationWindowManager,
                    GameService.Overlay,
                    this.dependencyInjectionContainer.FormattedLabelHtmlService,
                    this.achievementOverviewView)
                {
                    Parent = GameService.Graphics.SpriteScreen,
                };

                var savedWindowLocation = this.dependencyInjectionContainer.PersistanceService.Get();

                this.logger.Info($"SavedWindowLocation -  X:{savedWindowLocation.TrackWindowLocationX} Y:{savedWindowLocation.TrackWindowLocationY}");

                this.window.Location =
                    savedWindowLocation.TrackWindowLocationX == -1 || savedWindowLocation.TrackWindowLocationY == -1 ?
                    (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2)) :
                    new Point(savedWindowLocation.TrackWindowLocationX, savedWindowLocation.TrackWindowLocationY);

                this.logger.Info($"AchievementTrackWindowLocation -  X:{this.window.Location.X} Y:{this.window.Location.Y}");
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

        // The other prefetch trigger is AchievementDetailsWindowManager.CreateWindow ("opened in
        // detail") -- tracking an achievement is worth aligning even before its detail/Track window
        // control is ever built, since the Track window panel's embedded control needs it too.
        private void PrefetchBitAlignment(int achievementId)
        {
            var achievement = this.dependencyInjectionContainer.AchievementService.Achievements?.FirstOrDefault(x => x.Id == achievementId);

            if (achievement != null)
            {
                _ = this.dependencyInjectionContainer.BitAlignmentService.PrefetchAsync(achievementId, achievement);
            }
        }

        protected override void OnModuleLoaded(EventArgs e) =>
            base.OnModuleLoaded(e);

        protected override void Update(GameTime gameTime)
        {
            this.dependencyInjectionContainer?.ItemDetailWindowManager?.Update();
            this.dependencyInjectionContainer?.AchievementDetailsWindowManager?.Update();

            if (GameService.Gw2Mumble.IsAvailable && this.window != null)
            {
                if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsMapOpen)
                {
                    if (this.window.Visible)
                    {
                        this.purposelyHidden = true;
                        this.window.Hide();
                    }
                }
                else if (this.purposelyHidden)
                {
                    this.window.Show();
                    this.purposelyHidden = false;
                }
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            this.SavePersistentInformation();
            var location = this.window?.Location ?? new Point(-1, -1);
            this.dependencyInjectionContainer.PersistanceService?.Save(location.X, location.Y, this.window?.Visible ?? false);
            GameService.Overlay.BlishHudWindow.RemoveTab(this.blishhudOverlayTab);
            this.cornerIcon?.Dispose();
            this.window?.Dispose();
            this.dependencyInjectionContainer.TextureService?.Dispose();
            this.dependencyInjectionContainer.BitAlignmentService?.Dispose();
        }

        private void SavePersistentInformation()
        {
            var location = this.window?.Location ?? new Point(-1, -1);
            this.dependencyInjectionContainer.PersistanceService?.Save(location.X, location.Y, this.window?.Visible ?? false);
        }
    }
}
