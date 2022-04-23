using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementControlProvider
    {
        Control GetAchievementControl(Achievement achievement, AchievementTableEntryDescription description, Point size);
    }
}
