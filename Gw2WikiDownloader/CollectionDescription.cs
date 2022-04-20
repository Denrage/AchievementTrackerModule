// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("CollectionItems: {EntryList.Count} || {GameText} || {GameHint}")]
    public class CollectionDescription : AchievementTableEntryDescription
    {
        public List<CollectionDescriptionEntry> EntryList { get; set; } = new List<CollectionDescriptionEntry>();
    }
}
