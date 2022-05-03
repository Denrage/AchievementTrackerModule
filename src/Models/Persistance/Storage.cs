using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Models.Persistance
{
    public class Storage
    {
        public Dictionary<int, AchievementWindowInformation> AchievementInformation { get; set; } = new Dictionary<int, AchievementWindowInformation>();

        public Dictionary<int, Dictionary<int, ItemInformation>> ItemInformation { get; set; } = new Dictionary<int, Dictionary<int, ItemInformation>>();

        public List<int> TrackedAchievements { get; set; } = new List<int>();

        public int TrackWindowLocationX { get; set; } = -1;

        public int TrackWindowLocationY { get; set; } = -1;

        public bool ShowTrackWindow { get; set; } = false;
    }
}
