using Denrage.AchievementTrackerModule.UserInterface.Views;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementListItemFactory
    {
        AchievementListItem Create(Achievement achievement, string icon);
    }
}
