using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public abstract class AchievementControlFactory
    {
        public abstract Control Create(AchievementTableEntry achievement, object description);
    }
}
