using System.Collections.Generic;
using System.Diagnostics;

namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    [DebuggerDisplay("Objectives: {EntryList.Count} || {GameText} || {GameHint}")]
    public class ObjectivesDescription : AchievementTableEntryDescription
    {
        public List<TableDescriptionEntry> EntryList { get; set; } = new List<TableDescriptionEntry>();
    }
}
