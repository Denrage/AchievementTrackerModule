using Denrage.AchievementTrackerModule.UserInterface.Windows;
using System.Collections.Generic;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IItemDetailWindowFactory
    {
        ItemDetailWindow Create(string name, string[] columns, List<CollectionAchievementTableEntry> item, string achievementLink);
    }
}
