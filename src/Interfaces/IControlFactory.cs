using Denrage.AchievementTrackerModule.Models.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IControlFactory<T, TDescription>
            where T : IAchievementControl
    {
        T Create(AchievementTableEntry achievement, TDescription description);
    }
}
