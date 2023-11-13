using Autofac;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Services;
using Denrage.AchievementTrackerModule.Services.Factories;
using Denrage.AchievementTrackerModule.Services.Factories.ItemDetails;
using Denrage.AchievementTrackerModule.UserInterface.Views;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly IContainer container;
        private AchievementTrackWindow window;
        private CornerIcon cornerIcon;
        private bool purposelyHidden;

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
            var builder = new ContainerBuilder();
            
            builder.RegisterInstance(this.Gw2ApiManager);
            builder.RegisterInstance(this.ContentsManager);
            builder.RegisterInstance(GameService.Content);
            builder.RegisterInstance(this.DirectoriesManager);
            builder.RegisterInstance(Logger);
            builder.RegisterInstance(GameService.Graphics);
            builder.RegisterInstance(GameService.Overlay);
            builder.RegisterType<ExternalImageService>().As<IExternalImageService>().SingleInstance();
            builder.RegisterType<TextureService>().As<ITextureService>().SingleInstance();
            builder.RegisterType<AchievementService>().As<IAchievementService>().AsSelf().SingleInstance();
            builder.RegisterType<SubPageInformationWindowManager>().As<ISubPageInformationWindowManager>().SingleInstance();
            builder.RegisterType<FormattedLabelHtmlService>().As<IFormattedLabelHtmlService>().SingleInstance();
            builder.RegisterType<AchievementTrackerService>().As<IAchievementTrackerService>().AsSelf().SingleInstance();
            builder.RegisterType<AchievementItemOverviewFactory>().As<IAchievementItemOverviewFactory>().SingleInstance();
            builder.RegisterType<AchievementListItemFactory>().As<IAchievementListItemFactory>().SingleInstance();
            builder.RegisterType<AchievementTableEntryProvider>().As<IAchievementTableEntryProvider>().SingleInstance();
            builder.RegisterType<ItemDetailWindowFactory>().As<IItemDetailWindowFactory>().SingleInstance();
            builder.RegisterType<ItemDetailWindowManager>().As<IItemDetailWindowManager>().AsSelf().SingleInstance();
            builder.RegisterType<AchievementControlProvider>().As<IAchievementControlProvider>().SingleInstance();
            builder.RegisterType<AchievementControlManager>().As<IAchievementControlManager>().SingleInstance();
            builder.RegisterType<AchievementDetailsWindowFactory>().As<IAchievementDetailsWindowFactory>().SingleInstance();
            builder.RegisterType<AchievementDetailsWindowManager>().As<IAchievementDetailsWindowManager>().AsSelf().SingleInstance();        
            builder.RegisterType<PersistanceService>().As<IPersistanceService>().SingleInstance();
            builder.RegisterType<Services.SettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<BlishTabNavigationService>().As<IBlishTabNavigationService>().SingleInstance();
            
            builder.RegisterType<AchievementTrackerView>().AsSelf().SingleInstance();
            builder.RegisterType<AchievementTrackWindow>().AsSelf().SingleInstance();

            this.container = builder.Build();
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            var settingsService = this.container.Resolve<ISettingsService>();
            settingsService.AutoSave = settings.DefineSetting("AutoSave", false, () => "Auto save every 5 minutes", () => "Auto save tracked achievements, windows and their positions every 5 minutes");

            settingsService.LimitAchievements = settings.DefineSetting("LimitAchievements", true, () => "Limit Achievements to 15", () => "This will limit the maximum of achievements to 15. If it's disabled expect performance and usability issues.");
        }

        protected override void Initialize()
        {
            this.Gw2ApiManager.SubtokenUpdated += async (_, args) =>
            {
                this.container.Resolve<Logger>().Info("Subtoken updated");
                await this.container.Resolve<IAchievementService>().LoadPlayerAchievements();
            };
        }

        protected override async Task LoadAsync()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await this.container.Resolve<AchievementService>().LoadAsync();
                    this.container.Resolve<AchievementDetailsWindowManager>().Load(this.container.Resolve<IPersistanceService>());
                    this.container.Resolve<ItemDetailWindowManager>().Load(this.container.Resolve<IPersistanceService>());
                    this.container.Resolve<AchievementTrackerService>().Load(this.container.Resolve<IPersistanceService>());
                    this.container.Resolve<IBlishTabNavigationService>().Initialize();

                    this.container.Resolve<IAchievementTrackerService>().AchievementTracked += this.AchievementTrackerService_AchievementTracked;

                    if (this.container.Resolve<IPersistanceService>().Get().ShowTrackWindow)
                    {
                        this.InitializeWindow();
                        this.window.Show();
                    }

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

                    this.container.Resolve<IPersistanceService>().AutoSave += this.SavePersistentInformation;
                }
                catch (Exception ex)
                {

                    throw;
                }

            });

            await base.LoadAsync();
        }

        private void InitializeWindow()
        {
            if (this.window is null)
            {
                this.window = this.container.Resolve<AchievementTrackWindow>();
                this.window.Parent = GameService.Graphics.SpriteScreen;

                var savedWindowLocation = this.container.Resolve<IPersistanceService>().Get();

                this.container.Resolve<Logger>().Info($"SavedWindowLocation -  X:{savedWindowLocation.TrackWindowLocationX} Y:{savedWindowLocation.TrackWindowLocationY}");

                this.window.Location =
                    savedWindowLocation.TrackWindowLocationX == -1 || savedWindowLocation.TrackWindowLocationY == -1 ?
                    (GameService.Graphics.SpriteScreen.Size / new Point(2)) - (new Point(256, 178) / new Point(2)) :
                    new Point(savedWindowLocation.TrackWindowLocationX, savedWindowLocation.TrackWindowLocationY);

                this.container.Resolve<Logger>().Info($"AchievementTrackWindowLocation -  X:{this.window.Location.X} Y:{this.window.Location.Y}");
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
            this.container.Resolve<IItemDetailWindowManager>().Update();
            this.container.Resolve<IAchievementDetailsWindowManager>().Update();

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
            this.container.Resolve<IPersistanceService>().Save(location.X, location.Y, this.window?.Visible ?? false);
            this.cornerIcon?.Dispose();
            this.window?.Dispose();
            this.container.Resolve<ITextureService>().Dispose();
            this.container.Resolve<IBlishTabNavigationService>().Dispose();
        }

        private void SavePersistentInformation()
        {
            var location = this.window?.Location ?? new Point(-1, -1);
            this.container.Resolve<IPersistanceService>().Save(location.X, location.Y, this.window?.Visible ?? false);
        }
    }
}
