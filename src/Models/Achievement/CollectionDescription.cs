using System.Collections.Generic;
using System.Diagnostics;

namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    [DebuggerDisplay("CollectionItems: {EntryList.Count} || {GameText} || {GameHint}")]
    public class CollectionDescription : AchievementTableEntryDescription
    {
        public List<CollectionDescriptionEntry> EntryList { get; set; } = new List<CollectionDescriptionEntry>();
    }
}
