// See https://aka.ms/new-console-template for more information
using System.Diagnostics;


namespace Gw2WikiDownload
{
    [DebuggerDisplay("{Tiers[0].DisplayName}")]
    public class MultiTierReward : Reward
    {
        public List<TierReward> Tiers { get; set; } = new List<TierReward>();

        public class TierReward : ItemReward
        {
            public int Tier { get; set; } = default;
        }
    }
}
