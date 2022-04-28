using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Services.Factories;
using Denrage.AchievementTrackerModule.Services.Factories.ItemDetails;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class DependencyInjectionContainer
    {
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly ContentsManager contentsManager;
        private readonly ContentService contentService;

        public IAchievementTrackerService AchievementTrackerService { get; set; }

        public IAchievementApiService AchievementApiService { get; set; }

        public IAchievementListItemFactory AchievementListItemFactory { get; set; }

        public IAchievementItemOverviewFactory AchievementItemOverviewFactory { get; set; }

        public IAchievementService AchievementService { get; set; }

        public IAchievementControlProvider AchievementControlProvider { get; set; }

        public IAchievementTableEntryProvider AchievementTableEntryProvider { get; set; }

        public IItemDetailWindowFactory ItemDetailWindowFactory { get; set; }

        public IAchievementDetailsWindowFactory AchievementDetailsWindowFactory { get; set; }

        public DependencyInjectionContainer(Gw2ApiManager gw2ApiManager, ContentsManager contentsManager, ContentService contentService)
        {
            this.gw2ApiManager = gw2ApiManager;
            this.contentsManager = contentsManager;
            this.contentService = contentService;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var apiService = new AchievementApiService(this.gw2ApiManager);
            var achievementService = new AchievementService(this.contentsManager, this.gw2ApiManager);
            this.AchievementService = achievementService;
            this.AchievementApiService = apiService;

            this.AchievementTrackerService = new AchievementTrackerService();
            this.AchievementListItemFactory = new AchievementListItemFactory(this.AchievementTrackerService, this.contentService, this.AchievementService);
            this.AchievementItemOverviewFactory = new AchievementItemOverviewFactory(this.AchievementListItemFactory, this.AchievementService);
            this.AchievementTableEntryProvider = new AchievementTableEntryProvider(this.AchievementService);
            this.ItemDetailWindowFactory = new ItemDetailWindowFactory(this.contentsManager, this.AchievementService, this.AchievementTableEntryProvider);
            this.AchievementControlProvider = new AchievementControlProvider(this.AchievementService, this.ItemDetailWindowFactory, this.contentsManager);
            this.AchievementDetailsWindowFactory = new AchievementDetailsWindowFactory(this.contentsManager, this.AchievementService, this.AchievementControlProvider);

            await apiService.LoadAsync(cancellationToken);
            await achievementService.LoadAsync(cancellationToken);
        }
    }
}
