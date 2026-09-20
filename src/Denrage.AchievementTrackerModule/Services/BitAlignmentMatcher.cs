using Denrage.AchievementTrackerModule.Libs.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Denrage.AchievementTrackerModule.Services
{
    // Pure, no Blish/Gw2Sharp-client/UI dependency -- keeps the matching logic testable independently
    // of BitAlignmentService, which owns everything that needs the network.
    public static class BitAlignmentMatcher
    {
        // One wiki collection/objective row: its display name (for text/skin/minipet matching) and, for
        // collection rows only, the wiki's own notion of an item id (0 = unknown -- ObjectivesDescription
        // rows never have one).
        public readonly struct Row
        {
            public Row(string displayName, int id)
            {
                this.DisplayName = displayName;
                this.Id = id;
            }

            public string DisplayName { get; }

            public int Id { get; }
        }

        // Wiki EntryList index -> API bits index, in wiki row order. -1 = unresolved. Returns null if the
        // description isn't a row-indexed achievement type (nothing to align).
        public static IReadOnlyList<Row> GetRows(AchievementTableEntryDescription description)
        {
            switch (description)
            {
                case CollectionDescription collection:
                    return collection.EntryList.Select(e => new Row(e.DisplayName, e.Id)).ToList();
                case ObjectivesDescription objectives:
                    return objectives.EntryList.Select(e => new Row(e.DisplayName, 0)).ToList();
                default:
                    return null;
            }
        }

        // Matching order, each bit consumed at most once:
        //   1. Text bits <-> row DisplayName, by normalised string.
        //   2. Item bits <-> row Id (the wiki's own item id), by value.
        //   3. Skin/Minipet bits <-> row DisplayName, by normalised name resolved from the bit's item id
        //      via skinNamesById/miniNamesById (a different id namespace than #2, so it needs its own pass).
        //   4. Position fallback: only when row and bit counts are equal and both ends are still unclaimed.
        public static int[] ComputeRowToBit(
            IReadOnlyList<Row> rows,
            IReadOnlyList<AchievementBit> bits,
            IReadOnlyDictionary<int, string> skinNamesById,
            IReadOnlyDictionary<int, string> miniNamesById)
        {
            var rowToBit = new int[rows.Count];
            for (var i = 0; i < rowToBit.Length; i++)
            {
                rowToBit[i] = -1;
            }

            if (bits.Count == 0)
            {
                return rowToBit;
            }

            var bitClaimed = new bool[bits.Count];
            var bitText = new string[bits.Count];
            var bitItemId = new int?[bits.Count];
            var bitResolvedName = new string[bits.Count];

            for (var i = 0; i < bits.Count; i++)
            {
                switch (bits[i])
                {
                    case AchievementTextBit textBit:
                        bitText[i] = Normalize(textBit.Text);
                        break;
                    case AchievementItemBit itemBit:
                        bitItemId[i] = itemBit.Id;
                        break;
                    case AchievementSkinBit skinBit:
                        bitItemId[i] = skinBit.Id;
                        if (skinNamesById.TryGetValue(skinBit.Id, out var skinName))
                        {
                            bitResolvedName[i] = Normalize(skinName);
                        }
                        break;
                    case AchievementMinipetBit minipetBit:
                        bitItemId[i] = minipetBit.Id;
                        if (miniNamesById.TryGetValue(minipetBit.Id, out var miniName))
                        {
                            bitResolvedName[i] = Normalize(miniName);
                        }
                        break;
                }
            }

            // Pass 1: text bits by normalised DisplayName.
            for (var row = 0; row < rows.Count; row++)
            {
                var name = Normalize(rows[row].DisplayName);
                if (name is null)
                {
                    continue;
                }

                for (var bit = 0; bit < bits.Count; bit++)
                {
                    if (!bitClaimed[bit] && bitText[bit] == name)
                    {
                        rowToBit[row] = bit;
                        bitClaimed[bit] = true;
                        break;
                    }
                }
            }

            // Pass 2: item bits by the wiki row's own item id (plain Item bits only -- Skin/Minipet ids
            // live in a different namespace and are matched by resolved name in pass 3 instead).
            for (var row = 0; row < rows.Count; row++)
            {
                if (rowToBit[row] != -1 || rows[row].Id <= 0)
                {
                    continue;
                }

                for (var bit = 0; bit < bits.Count; bit++)
                {
                    if (!bitClaimed[bit] && bitResolvedName[bit] is null && bitItemId[bit] == rows[row].Id)
                    {
                        rowToBit[row] = bit;
                        bitClaimed[bit] = true;
                        break;
                    }
                }
            }

            // Pass 3: skin/minipet bits by resolved name vs. row DisplayName.
            for (var row = 0; row < rows.Count; row++)
            {
                if (rowToBit[row] != -1)
                {
                    continue;
                }

                var name = Normalize(rows[row].DisplayName);
                if (name is null)
                {
                    continue;
                }

                for (var bit = 0; bit < bits.Count; bit++)
                {
                    if (!bitClaimed[bit] && bitResolvedName[bit] == name)
                    {
                        rowToBit[row] = bit;
                        bitClaimed[bit] = true;
                        break;
                    }
                }
            }

            // Pass 4: position fallback, only when the achievement's row/bit counts genuinely match.
            if (rows.Count == bits.Count)
            {
                for (var row = 0; row < rows.Count; row++)
                {
                    if (rowToBit[row] == -1 && !bitClaimed[row])
                    {
                        rowToBit[row] = row;
                        bitClaimed[row] = true;
                    }
                }
            }

            return rowToBit;
        }

        public static bool IsIdentity(int[] rowToBit)
        {
            for (var i = 0; i < rowToBit.Length; i++)
            {
                if (rowToBit[i] != i)
                {
                    return false;
                }
            }

            return true;
        }

        // Trim, case-fold, collapse whitespace, strip a trailing wiki footnote marker ("[1]") and a
        // trailing period, so "Ashen Skerries[1]" (wiki) and "Ashen Skerries" (API) compare equal.
        internal static string Normalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var result = Regex.Replace(value, @"\s+", " ").Trim();
            result = Regex.Replace(result, @"\[\d+\]\s*$", "").TrimEnd();

            if (result.EndsWith("."))
            {
                result = result.Substring(0, result.Length - 1).TrimEnd();
            }

            return result.ToLowerInvariant();
        }
    }
}
