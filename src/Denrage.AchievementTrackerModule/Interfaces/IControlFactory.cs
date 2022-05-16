using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IControlFactory<T, TDescription>
            where T : IAchievementControl
    {
        T Create(AchievementTableEntry achievement, TDescription description);
    }
}
