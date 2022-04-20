using System.Diagnostics;


namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    [DebuggerDisplay("{DisplayName}")]
    public class CollectionDescriptionEntry
    {
        public string DisplayName { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public int Id { get; set; } = 0;
    }
}
