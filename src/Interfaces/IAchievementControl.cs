using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementControl
    {
        void BuildControl();

        Point Size { get; set; }
    }
}
