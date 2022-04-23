using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementApiService : IAchievementApiService
    {
        private readonly Gw2ApiManager gw2ApiManager;

        public IEnumerable<AchievementGroup> AchievementGroups { get; private set; }

        public IEnumerable<AchievementCategory> AchievementCategories { get; private set; }

        public AchievementApiService(Gw2ApiManager gw2ApiManager)
        {
            this.gw2ApiManager = gw2ApiManager;
        }

        public async Task LoadAsync()
        {
            this.AchievementGroups = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.Groups.AllAsync();
            this.AchievementCategories = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.Categories.AllAsync();
        }
    }
}
