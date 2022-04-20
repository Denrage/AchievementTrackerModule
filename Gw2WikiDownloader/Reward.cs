// See https://aka.ms/new-console-template for more information

namespace Gw2WikiDownload
{
    public abstract class Reward
    {
        public static Reward EmptyReward { get; } = new EmptyReward();
    }
}
