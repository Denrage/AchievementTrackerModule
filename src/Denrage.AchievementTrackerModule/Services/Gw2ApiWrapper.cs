using Autofac;
using Blish_HUD;
using Blish_HUD.Gw2WebApi;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Denrage.AchievementTrackerModule.Services
{
    public class Gw2ApiWrapper : IGw2ApiPermission
    {
        private const string SUBTOKEN_CLAIMTYPE = "permissions";
        private readonly Logger logger;
        private readonly ManagedConnection connection;
        private readonly JwtSecurityTokenHandler subtokenHandler;

        private HashSet<TokenPermission> activePermissions;

        public IGw2WebApiV2Client WebClient => connection.Client.V2;

        public event EventHandler<ValueEventArgs<IEnumerable<TokenPermission>>> SubtokenUpdated;

        public Gw2ApiWrapper(Logger logger, ManagedConnection connection)
        {
            this.logger = logger;
            this.connection = connection;
            activePermissions = new HashSet<TokenPermission>();
            subtokenHandler = new JwtSecurityTokenHandler();
        }

        public void SetToken(string responseToken)
        {
            activePermissions.Clear();
            if (connection.SetApiKey(responseToken) && subtokenHandler.CanReadToken(responseToken))
            {
                try
                {
                    var jwtToken = subtokenHandler.ReadJwtToken(responseToken);

                    activePermissions = jwtToken.Claims.Where(x => x.Type.Equals(SUBTOKEN_CLAIMTYPE) && Enum.TryParse(x.Value, true, out TokenPermission _))
                                                 .Select(y => (TokenPermission)Enum.Parse(typeof(TokenPermission), y.Value, true))
                                                 .ToHashSet();

                    // TODO: consider checking against the expiration claim.

                    SubtokenUpdated?.Invoke(this, new ValueEventArgs<IEnumerable<TokenPermission>>(activePermissions));
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Failed to parse API subtoken.");
                }
            }
        }

        public bool HasPermissions(IEnumerable<TokenPermission> permissions)
        {
            return activePermissions.IsSupersetOf(permissions);
        }

        public bool HasPermission(TokenPermission permission)
        {
            return activePermissions.Contains(permission);
        }
    }
}
