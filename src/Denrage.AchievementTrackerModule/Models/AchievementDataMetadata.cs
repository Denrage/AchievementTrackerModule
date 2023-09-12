namespace Denrage.AchievementTrackerModule.Models
{
    public class AchievementDataMetadata
    {
        public int Version { get; set; }

        public string AchievementDataMd5 { get; set; }
        
        public string AchievementTablesMd5 { get; set; }
        
        public string SubPagesMd5 { get; set; }
    }
}
