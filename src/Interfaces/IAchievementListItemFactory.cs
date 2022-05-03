using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Views;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementListItemFactory
    {
        AchievementListItem Create(AchievementTableEntry achievement, string icon);
    }
}
