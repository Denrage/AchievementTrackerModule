using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Services.Factories.ItemDetails;
using System;
using System.Collections.Generic;
using static Denrage.AchievementTrackerModule.Libs.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services
{
    public class AchievementTableEntryProvider : IAchievementTableEntryProvider
    {
        private readonly Dictionary<Type, AchievementTableEntryFactory> mapping = new Dictionary<Type, AchievementTableEntryFactory>();

        public AchievementTableEntryProvider(IFormattedLabelHtmlService formattedLabelHtmlService, IExternalImageService externalImageService, Logger logger)
        {
            this.mapping.Add(typeof(CollectionAchievementTableNumberEntry), new AchievementTableNumberEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableCoinEntry), new AchievementTableCoinEntryFactory());
            this.mapping.Add(typeof(CollectionAchievementTableItemEntry), new AchievementTableItemEntryFactory(externalImageService));
            this.mapping.Add(typeof(CollectionAchievementTableLinkEntry), new AchievementTableLinkEntryFactory(formattedLabelHtmlService));
            this.mapping.Add(typeof(CollectionAchievementTableMapEntry), new AchievementTableMapEntryFactory(externalImageService, logger));
            this.mapping.Add(typeof(CollectionAchievementTableStringEntry), new AchievementTableStringEntryFactory(formattedLabelHtmlService));
            this.mapping.Add(typeof(CollectionAchievementTableEmptyEntry), new AchievementTableEmptyEntryFactory());
        }

        public Control GetTableEntryControl(CollectionAchievementTableEntry entry)
            => this.mapping.TryGetValue(entry.GetType(), out var factory) ? factory.Create(entry) : null;
    }
}
