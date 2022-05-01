using Denrage.AchievementTrackerModule.Interfaces;
using System.Diagnostics;

namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    [DebuggerDisplay("{DisplayName}")]
    public class TableDescriptionEntry : ILinkEntry
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Link { set; get; } = string.Empty;
    }
}
