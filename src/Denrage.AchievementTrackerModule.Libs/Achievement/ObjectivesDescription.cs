using System.Collections.Generic;
using System.Diagnostics;

namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    [DebuggerDisplay("Objectives: {EntryList.Count} || {GameText} || {GameHint}")]
    public class ObjectivesDescription : AchievementTableEntryDescription
    {
        public List<TableDescriptionEntry> EntryList { get; set; } = new List<TableDescriptionEntry>();
    }
}
