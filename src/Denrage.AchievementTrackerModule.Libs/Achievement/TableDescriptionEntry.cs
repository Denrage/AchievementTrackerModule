using Denrage.AchievementTrackerModule.Libs.Interfaces;
using System.Diagnostics;

namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    [DebuggerDisplay("{DisplayName}")]
    public class TableDescriptionEntry : ILinkEntry
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Link { set; get; } = string.Empty;
    }
}
