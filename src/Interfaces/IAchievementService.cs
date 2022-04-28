using Blish_HUD.Content;
using Denrage.AchievementTrackerModule.Models.Achievement;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementService
    {
        IReadOnlyList<AchievementTableEntry> Achievements { get; }

        IReadOnlyList<CollectionAchievementTable> AchievementDetails { get; }

        event Action PlayerAchievementsLoaded;

        Task<string> GetDirectImageLink(string imagePath, CancellationToken cancellationToken = default);
        
        AsyncTexture2D GetImage(string imageUrl);
        
        AsyncTexture2D GetImageFromIndirectLink(string imagePath);
        
        bool HasFinishedAchievement(int achievementId);
        
        bool HasFinishedAchievementBit(int achievementId, int positionIndex);

        Task LoadPlayerAchievements(CancellationToken cancellationToken = default);
    }
}
