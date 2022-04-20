// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("{DisplayName}")]
    public class ItemReward : Reward
    {
        public string ImageUrl { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string ItemUrl { get; set; } = string.Empty;

        public int Id { get; set; } = default;
    }
}
