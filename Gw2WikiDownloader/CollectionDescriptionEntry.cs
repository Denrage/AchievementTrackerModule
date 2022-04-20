// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
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
