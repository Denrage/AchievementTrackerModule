using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    // Depends only on IAchievementService, Gw2ApiManager and Logger, plus the Libs models -- nothing
    // else in the module. The actual row/bit matching logic lives in the dependency-free BitAlignmentMatcher.
    public class BitAlignmentService : IBitAlignmentService
    {
        private const int ApiBatchSize = 200;

        // The ten ids specialSnowflakeCompletedHandling used to hand-map (a position-as-bit-index table
        // for achievements where the wiki and the API disagree). Kept here only to log a one-time
        // comparison against the generated map -- see RunValidationAsync.
        private static readonly Dictionary<int, Func<int, int>> LegacySpecialSnowflakeTable = new Dictionary<int, Func<int, int>>()
        {
            { 5693, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 6 : index == 4 ? 7 : index == 5 ? 8 : -1  },
            { 5700, index => index == 0 ? 1 : index == 1 ? 2 : index == 2 ? 5 : index == 3 ? 8 : -1  },
            { 5704, index => index == 0 ? 1 : index == 1 ? 2 : index == 2 ? 5 : index == 3 ? 8 : -1  },
            { 5703, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 3 : index == 4 ? 4 : index == 5 ? 5 : index == 6 ? 7 : -1  },
            { 5697, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 3 : index == 3 ? 5 : index == 4 ? 6 : -1  },
            { 5688, index => index == 0 ? 3 : index == 1 ? 4 : index == 2 ? 6 : index == 3 ? 7 : index == 4 ? 8 : -1  },
            { 5709, index => index == 0 ? 0 : index == 1 ? 2 : index == 2 ? 4 : index == 3 ? 6 : index == 4 ? 7 : -1  },
            { 5698, index => index == 0 ? 0 : index == 1 ? 3 : index == 2 ? 4 : index == 3 ? 6 : -1  },
            { 5691, index => index == 0 ? 4 : index == 1 ? 5 : index == 2 ? 6 : index == 3 ? 7 : -1  },
            { 5708, index => index == 0 ? 0 : index == 1 ? 1 : index == 2 ? 2 : index == 3 ? 5 : index == 4 ? 8 : -1  },
        };

        // The generated map matches the legacy table exactly for nine of the ten ids above. The tenth,
        // 5691 ("Tide Turner: Caledon Forest"), disagrees on rows 0-1 (legacy bit 4/5, generated bit
        // 3/4; rows 2-3 agree) -- kept as a one-id override rather than trusting the generic matcher there.
        private static readonly Dictionary<int, Func<int, int>> ManualOverrides = new Dictionary<int, Func<int, int>>()
        {
            { 5691, index => index == 0 ? 4 : index == 1 ? 5 : index == 2 ? 6 : index == 3 ? 7 : -1  },
        };

        private readonly IAchievementService achievementService;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly Logger logger;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly object achievementCacheLock = new object();
        private readonly Dictionary<int, Achievement> achievementCacheById = new Dictionary<int, Achievement>();

        private readonly object nameCacheLock = new object();
        private readonly Dictionary<int, string> skinNamesById = new Dictionary<int, string>();
        private readonly Dictionary<int, string> miniNamesById = new Dictionary<int, string>();

        private readonly object alignmentLock = new object();
        private readonly Dictionary<int, int[]> rowToBitByAchievementId = new Dictionary<int, int[]>();
        private readonly HashSet<int> alignmentInFlight = new HashSet<int>();

        public event Action<int> AlignmentLoaded;

        public BitAlignmentService(IAchievementService achievementService, Gw2ApiManager gw2ApiManager, Logger logger)
        {
            this.achievementService = achievementService;
            this.gw2ApiManager = gw2ApiManager;
            this.logger = logger;
        }

        public void Dispose()
            => this.cts.Cancel();

        public int MapRowToBit(int achievementId, int rowIndex)
        {
            lock (this.alignmentLock)
            {
                if (this.rowToBitByAchievementId.TryGetValue(achievementId, out var map) && rowIndex >= 0 && rowIndex < map.Length)
                {
                    return map[rowIndex];
                }
            }

            // No alignment computed (yet, or ever, e.g. no API bits at all) -- fall back to the row index.
            return rowIndex;
        }

        public async Task PrefetchAsync(int achievementId, AchievementTableEntry achievement, CancellationToken cancellationToken = default)
        {
            lock (this.alignmentLock)
            {
                if (this.rowToBitByAchievementId.ContainsKey(achievementId) || !this.alignmentInFlight.Add(achievementId))
                {
                    return;
                }
            }

            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.cts.Token))
            {
                try
                {
                    var rowToBit = await this.GetOrComputeRowToBitAsync(achievement, linked.Token);

                    if (rowToBit != null)
                    {
                        this.AlignmentLoaded?.Invoke(achievementId);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Module was disabled while the fetch was in flight.
                }
                catch (Exception ex)
                {
                    this.logger.Warn(ex, $"BitAlignmentService: failed to align achievement {achievementId}; falling back to identity mapping.");
                }
                finally
                {
                    lock (this.alignmentLock)
                    {
                        _ = this.alignmentInFlight.Remove(achievementId);
                    }
                }
            }
        }

        private async Task<int[]> GetOrComputeRowToBitAsync(AchievementTableEntry achievement, CancellationToken cancellationToken)
        {
            lock (this.alignmentLock)
            {
                if (this.rowToBitByAchievementId.TryGetValue(achievement.Id, out var cached))
                {
                    return cached;
                }
            }

            var rows = BitAlignmentMatcher.GetRows(achievement.Description);
            if (rows is null || rows.Count == 0)
            {
                return null;
            }

            var achievements = await this.GetAchievementsAsync(new[] { achievement.Id }, cancellationToken);

            if (!achievements.TryGetValue(achievement.Id, out var apiAchievement) || apiAchievement.Bits is null || apiAchievement.Bits.Count == 0)
            {
                return null;
            }

            var skinIds = apiAchievement.Bits.OfType<AchievementSkinBit>().Select(b => b.Id).Distinct().ToList();
            var miniIds = apiAchievement.Bits.OfType<AchievementMinipetBit>().Select(b => b.Id).Distinct().ToList();

            await this.EnsureSkinNamesAsync(skinIds, cancellationToken);
            await this.EnsureMiniNamesAsync(miniIds, cancellationToken);

            IReadOnlyDictionary<int, string> skinNamesSnapshot;
            IReadOnlyDictionary<int, string> miniNamesSnapshot;

            lock (this.nameCacheLock)
            {
                skinNamesSnapshot = new Dictionary<int, string>(this.skinNamesById);
                miniNamesSnapshot = new Dictionary<int, string>(this.miniNamesById);
            }

            var rowToBit = BitAlignmentMatcher.ComputeRowToBit(rows, apiAchievement.Bits, skinNamesSnapshot, miniNamesSnapshot);

            if (ManualOverrides.TryGetValue(achievement.Id, out var overrideFunc))
            {
                for (var row = 0; row < rowToBit.Length; row++)
                {
                    var overridden = overrideFunc(row);
                    if (overridden != -1)
                    {
                        rowToBit[row] = overridden;
                    }
                }
            }

            if (!BitAlignmentMatcher.IsIdentity(rowToBit))
            {
                var unresolvedCount = rowToBit.Count(b => b == -1);
                this.logger.Info($"BitAlignmentService: achievement {achievement.Id} ('{achievement.Name}') needed row/bit alignment; {unresolvedCount} of {rowToBit.Length} row(s) unresolved.");
            }

            lock (this.alignmentLock)
            {
                this.rowToBitByAchievementId[achievement.Id] = rowToBit;
            }

            return rowToBit;
        }

        public async Task<IReadOnlyDictionary<int, Achievement>> GetAchievementsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            var idList = ids.Distinct().ToList();
            List<int> missing;

            lock (this.achievementCacheLock)
            {
                missing = idList.Where(id => !this.achievementCacheById.ContainsKey(id)).ToList();
            }

            for (var i = 0; i < missing.Count; i += ApiBatchSize)
            {
                var chunk = missing.Skip(i).Take(ApiBatchSize).ToList();

                try
                {
                    var fetched = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.ManyAsync(chunk, cancellationToken);

                    lock (this.achievementCacheLock)
                    {
                        foreach (var fetchedAchievement in fetched)
                        {
                            this.achievementCacheById[fetchedAchievement.Id] = fetchedAchievement;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Warn(ex, $"BitAlignmentService: failed to fetch {chunk.Count} achievement(s); they'll be missing from this pass' results.");
                }
            }

            lock (this.achievementCacheLock)
            {
                return idList
                    .Where(this.achievementCacheById.ContainsKey)
                    .ToDictionary(id => id, id => this.achievementCacheById[id]);
            }
        }

        private async Task EnsureSkinNamesAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken)
        {
            List<int> missing;
            lock (this.nameCacheLock)
            {
                missing = ids.Where(id => !this.skinNamesById.ContainsKey(id)).ToList();
            }

            if (missing.Count == 0)
            {
                return;
            }

            try
            {
                var skins = await this.gw2ApiManager.Gw2ApiClient.V2.Skins.ManyAsync(missing, cancellationToken);

                lock (this.nameCacheLock)
                {
                    foreach (var skin in skins)
                    {
                        this.skinNamesById[skin.Id] = skin.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, $"BitAlignmentService: failed to fetch {missing.Count} skin name(s).");
            }
        }

        private async Task EnsureMiniNamesAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken)
        {
            List<int> missing;
            lock (this.nameCacheLock)
            {
                missing = ids.Where(id => !this.miniNamesById.ContainsKey(id)).ToList();
            }

            if (missing.Count == 0)
            {
                return;
            }

            try
            {
                var minis = await this.gw2ApiManager.Gw2ApiClient.V2.Minis.ManyAsync(missing, cancellationToken);

                lock (this.nameCacheLock)
                {
                    foreach (var mini in minis)
                    {
                        this.miniNamesById[mini.Id] = mini.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Warn(ex, $"BitAlignmentService: failed to fetch {missing.Count} minipet name(s).");
            }
        }

        public async Task RunValidationAsync(CancellationToken cancellationToken = default)
        {
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.cts.Token))
            {
                var candidates = this.achievementService.Achievements
                    .Where(a => BitAlignmentMatcher.GetRows(a.Description) != null)
                    .ToList();

                this.logger.Info($"BitAlignmentService validation: checking {candidates.Count} collection/objective achievement(s)...");

                var checkedCount = 0;
                var unresolvedAchievements = 0;
                var totalUnresolvedRows = 0;
                var legacyMismatches = new List<string>();

                foreach (var achievement in candidates)
                {
                    linked.Token.ThrowIfCancellationRequested();

                    int[] rowToBit;

                    try
                    {
                        rowToBit = await this.GetOrComputeRowToBitAsync(achievement, linked.Token);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Warn(ex, $"BitAlignmentService validation: failed to align achievement {achievement.Id} ('{achievement.Name}').");
                        continue;
                    }

                    if (rowToBit is null)
                    {
                        // No API bits for this id at all -- expected for a handful of wiki entries; nothing
                        // to validate.
                        continue;
                    }

                    checkedCount++;
                    var unresolvedCount = rowToBit.Count(b => b == -1);

                    if (unresolvedCount > 0)
                    {
                        unresolvedAchievements++;
                        totalUnresolvedRows += unresolvedCount;
                    }

                    if (LegacySpecialSnowflakeTable.TryGetValue(achievement.Id, out var legacyFunc))
                    {
                        for (var row = 0; row < rowToBit.Length; row++)
                        {
                            var legacyBit = legacyFunc(row);
                            if (legacyBit != -1 && legacyBit != rowToBit[row])
                            {
                                legacyMismatches.Add($"id={achievement.Id} ('{achievement.Name}') row={row} legacy_bit={legacyBit} generated_bit={rowToBit[row]}");
                            }
                        }
                    }
                }

                this.logger.Info($"BitAlignmentService validation: checked {checkedCount} achievement(s) with API bits; {unresolvedAchievements} had at least one unresolved row ({totalUnresolvedRows} unresolved row(s) total).");

                if (legacyMismatches.Count == 0)
                {
                    this.logger.Info("BitAlignmentService validation: generated maps for the specialSnowflakeCompletedHandling ids match the legacy table exactly.");
                }
                else
                {
                    foreach (var mismatch in legacyMismatches)
                    {
                        this.logger.Warn($"BitAlignmentService validation: legacy/generated mismatch: {mismatch}");
                    }
                }
            }
        }
    }
}
