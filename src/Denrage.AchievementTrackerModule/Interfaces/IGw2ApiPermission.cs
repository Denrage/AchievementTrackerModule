using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IGw2ApiPermission
    {
        bool HasPermission(TokenPermission permission);

        bool HasPermissions(IEnumerable<TokenPermission> permissions);
    }
}
