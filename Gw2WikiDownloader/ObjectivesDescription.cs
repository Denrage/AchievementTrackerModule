// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("Objectives: {EntryList.Count} || {GameText} || {GameHint}")]
    public class ObjectivesDescription : AchievementTableEntryDescription
    {
        public List<TableDescriptionEntry> EntryList { get; set; } = new List<TableDescriptionEntry>();
    }
}
