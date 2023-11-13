using Blish_HUD;
using Blish_HUD.Gw2WebApi;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;

namespace Denrage.AchievementTrackerModule.Services
{
    public class PlayerAchievementServiceFactory
    {
        private readonly Logger logger;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly IPersistanceService persistanceService;
        private PlayerAchievementService ownPlayerAchievementServiceCache;

        public PlayerAchievementServiceFactory(Logger logger, Gw2ApiManager gw2ApiManager, IPersistanceService persistanceService)
        {
            this.logger = logger;
            this.gw2ApiManager = gw2ApiManager;
            this.persistanceService = persistanceService;
        }

        public PlayerAchievementService Create(ManagedConnection connection)
        {
            var wrapper = new Gw2ApiWrapper(logger, connection);
            return new PlayerAchievementService(logger, wrapper.WebClient, wrapper, null);
        }

        public PlayerAchievementService CreateOwn()
        {
            if (ownPlayerAchievementServiceCache is null)
            {
                ownPlayerAchievementServiceCache = new PlayerAchievementService(logger, gw2ApiManager.Gw2ApiClient.V2, new Gw2ApiManagerWrapper(gw2ApiManager), persistanceService);
            }
            return ownPlayerAchievementServiceCache;
        }
    }
}
