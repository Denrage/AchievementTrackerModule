using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using System.Collections.Generic;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class ItemDetailWindowFactory : IItemDetailWindowFactory
    {
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementTableEntryProvider achievementTableEntryProvider;

        public ItemDetailWindowFactory(ContentsManager contentsManager, IAchievementService achievementService, IAchievementTableEntryProvider achievementTableEntryProvider)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementTableEntryProvider = achievementTableEntryProvider;
        }

        public ItemDetailWindow Create(string name, string[] columns, List<CollectionAchievementTableEntry> item, string achievementLink)
            => new ItemDetailWindow(this.contentsManager, this.achievementService, this.achievementTableEntryProvider, achievementLink, name, columns, item);
    }
}
