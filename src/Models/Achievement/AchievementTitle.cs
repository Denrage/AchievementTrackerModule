using System.Diagnostics;


namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    [DebuggerDisplay("{Title}")]
    public class AchievementTitle
    {
        public static AchievementTitle EmptyTitle { get; } = new AchievementTitle() { Title = string.Empty };

        public string Title { get; set; } = string.Empty;
    }
}
