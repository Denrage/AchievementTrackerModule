namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    public abstract class Reward
    {
        public static Reward EmptyReward { get; } = new EmptyReward();
    }
}
