using Denrage.AchievementTrackerModule.Libs.Achievement;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IBitAlignmentService : IDisposable
    {
        // Fires once an achievement's row->bit map has been computed (or re-computed), so a reader that
        // rendered with the identity fallback (see MapRowToBit) can refresh. Always raised on a
        // background thread -- marshal before touching controls.
        event Action<int> AlignmentLoaded;

        // Shared id->Achievement cache (flags/tiers/bits), fetched in 200-id batches. HasFinishedAchievementBit
        // and RunValidationAsync both need it; this is the one place that fetches it.
        Task<IReadOnlyDictionary<int, Achievement>> GetAchievementsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

        // Kicks off (if not already cached or in flight) computing the row->bit map for one achievement.
        // Fire-and-forget from a caller's point of view -- results land via the cache (MapRowToBit) and
        // the AlignmentLoaded event, not this task's return value.
        Task PrefetchAsync(int achievementId, AchievementTableEntry achievement, CancellationToken cancellationToken = default);

        // Translates a wiki row index into the API's bit index for this achievement. Returns the row
        // index unchanged (identity) if no alignment has been computed yet, or -1 if alignment was
        // computed but this row couldn't be resolved to any bit.
        int MapRowToBit(int achievementId, int rowIndex);

        // Debug-setting-gated: aligns every collection/objective achievement the wiki data knows about
        // and logs a summary, so the fix can be checked without tracking/opening each one by hand.
        Task RunValidationAsync(CancellationToken cancellationToken = default);
    }
}
