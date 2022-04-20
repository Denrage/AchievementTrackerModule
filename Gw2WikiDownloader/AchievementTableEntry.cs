// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("{Name}")]
    public class AchievementTableEntry
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Link { get; set; } = null;

        public bool HasLink => !string.IsNullOrEmpty(this.Link);

        public string? Prerequisite { get; set; } = null;

        public AchievementTitle Title { get; set; } = AchievementTitle.EmptyTitle;

        public Reward Reward { get; set; } = Reward.EmptyReward;

        public AchievementTableEntryDescription? Description { get; set; } = null;

        public string? Cite { get; set; } = null;
    }
}
