using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public abstract class AchievementControlFactory
    {
        public abstract Control Create(AchievementTableEntry achievement, object description);
    }
}
