using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Services
{
    public class Gw2ApiManagerWrapper : IGw2ApiPermission
    {
        private readonly Gw2ApiManager gw2ApiManager;

        public Gw2ApiManagerWrapper(Gw2ApiManager gw2ApiManager)
        {
            this.gw2ApiManager = gw2ApiManager;
        }

        public bool HasPermission(TokenPermission permission)
        {
            return gw2ApiManager.HasPermission(permission);
        }

        public bool HasPermissions(IEnumerable<TokenPermission> permissions)
        {
            return gw2ApiManager.HasPermissions(permissions);
        }
    }
}
