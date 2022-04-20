using System.Collections.Generic;
using System.Diagnostics;


namespace Denrage.AchievementTrackerModule.Models.Achievement
{
    [DebuggerDisplay("{Name} Columns: {ColumnNames.Length} Rows: {Entries.Count}")]
    public class CollectionAchievementTable
    {
        public string Name { get; set; } = string.Empty;

        public int Id { get; set; } = 0;

        public string Link { get; set; } = string.Empty;

        public string[] ColumnNames { get; set; }

        public List<List<CollectionAchievementTableEntry>> Entries { get; set; }

        public abstract class CollectionAchievementTableEntry
        {
        }

        [DebuggerDisplay("EmptyEntry")]
        public class CollectionAchievementTableEmptyEntry : CollectionAchievementTableEntry
        {
        }

        [DebuggerDisplay("String: {Text}")]
        public class CollectionAchievementTableStringEntry : CollectionAchievementTableEntry
        {
            public string Text { get; set; } = string.Empty;
        }

        [DebuggerDisplay("Map: {ImageLink}")]
        public class CollectionAchievementTableMapEntry : CollectionAchievementTableEntry
        {
            public string ImageLink { get; set; } = string.Empty;
        }

        [DebuggerDisplay("Link: {Link}")]
        public class CollectionAchievementTableLinkEntry : CollectionAchievementTableStringEntry
        {
            public string Link { get; set; } = string.Empty;
        }

        [DebuggerDisplay("Item: {Name}")]
        public class CollectionAchievementTableItemEntry : CollectionAchievementTableEntry
        {
            public string Link { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string ImageUrl { get; set; } = string.Empty;

            public int Id { get; set; } = 0;
        }

        [DebuggerDisplay("Coin: {Type} {ItemId}")]
        public class CollectionAchievementTableCoinEntry : CollectionAchievementTableEntry
        {
            public enum TradingPostType
            {
                Buy = 0,
                Sell = 1,
            }

            public int ItemId { get; set; } = 0;

            public TradingPostType Type { get; set; } = TradingPostType.Buy;
        }

        [DebuggerDisplay("Number: {Number}")]
        public class CollectionAchievementTableNumberEntry : CollectionAchievementTableEntry
        {
            public int Number { get; set; } = 0;
        }
    }
}
