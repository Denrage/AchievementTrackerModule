﻿using Blish_HUD.Controls;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule.Services.Factories.ItemDetails
{
    public class AchievementTableLinkEntryFactory : AchievementTableEntryFactory<CollectionAchievementTableLinkEntry>
    {
        protected override Control CreateInternal(CollectionAchievementTableLinkEntry entry)
            => new Label()
            {
                Text = entry.Text,
                AutoSizeHeight = true,
                WrapText = true,
            };
    }
}