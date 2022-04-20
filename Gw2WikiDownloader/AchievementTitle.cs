// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("{Title}")]
    public class AchievementTitle
    {
        public static AchievementTitle EmptyTitle { get; } = new AchievementTitle() { Title = string.Empty };

        public string Title { get; set; } = string.Empty;
    }
}
