using Denrage.AchievementTrackerModule.UserInterface.Windows;

namespace Denrage.AchievementTrackerModule.Models
{
    public class ItemDetailWindowInformation
    {
        public ItemDetailWindow Window { get; set; }

        public int AchievementId { get; set; }

        public int ItemIndex { get; set; }

        public string Name { get; set; }

        public string Identifier { get; set; }
    }
}
