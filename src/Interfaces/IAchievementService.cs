using Blish_HUD.Content;
using Denrage.AchievementTrackerModule.Models.Achievement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementService
    {
        IReadOnlyList<AchievementTableEntry> Achievements { get; }

        IReadOnlyList<CollectionAchievementTable> AchievementDetails { get; }

        Task<string> GetDirectImageLink(string imagePath);
        
        AsyncTexture2D GetImage(string imageUrl);
        
        AsyncTexture2D GetImageFromIndirectLink(string imagePath);
        
        bool HasFinishedAchievement(int achievementId);
        
        bool HasFinishedAchievementBit(int achievementId, int positionIndex);

        Task LoadPlayerAchievements();
    }
}
