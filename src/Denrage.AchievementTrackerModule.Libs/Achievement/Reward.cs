namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    public abstract class Reward
    {
        public static Reward EmptyReward { get; } = new EmptyReward();
    }
}
