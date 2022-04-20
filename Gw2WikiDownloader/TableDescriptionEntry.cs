// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("{DisplayName}")]
    public class TableDescriptionEntry
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Link { set; get; } = string.Empty;
    }
}
