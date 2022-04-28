using Denrage.AchievementTrackerModule.Models.Achievement;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IItemDetailWindowManager
    {
        void CreateAndShowWindow(string name, string[] columns, List<CollectionAchievementTable.CollectionAchievementTableEntry> item, string achievementLink);

        bool ShowWindow(string name);

        void Update();
    }
}