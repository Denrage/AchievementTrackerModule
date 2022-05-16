using Denrage.AchievementTrackerModule.Libs.Interfaces;
using System.Diagnostics;

namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    [DebuggerDisplay("{DisplayName}")]
    public class CollectionDescriptionEntry : ILinkEntry
    {
        public string DisplayName { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string Link { get; set; } = string.Empty;

        public int Id { get; set; } = 0;
    }
}
