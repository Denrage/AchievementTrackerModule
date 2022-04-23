using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Services.Factories.ItemDetails;
using System;
using System.Collections.Generic;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTableEntryProvider : IAchievementTableEntryProvider
    {
        private readonly Dictionary<Type, AchievementTableEntryFactory> mapping = new Dictionary<Type, AchievementTableEntryFactory>();

        public AchievementTableEntryProvider(IAchievementService achievementService)
        {
            this.mapping.Add(typeof(CollectionAchievementTableNumberEntry), new AchievementTableNumberEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableCoinEntry), new AchievementTableCoinEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableItemEntry), new AchievementTableItemEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableLinkEntry), new AchievementTableLinkEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableMapEntry), new AchievementTableMapEntryFactory(achievementService));
            this.mapping.Add(typeof(CollectionAchievementTableStringEntry), new AchievementTableStringEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableEmptyEntry), new AchievementTableEmptyEntryFactory());
        }

        public Control GetTableEntryControl(CollectionAchievementTableEntry entry)
            => this.mapping.TryGetValue(entry.GetType(), out var factory) ? factory.Create(entry) : null;
    }
}
