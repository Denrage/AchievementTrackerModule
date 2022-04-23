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

        AsyncTexture2D GetDirectImageLink(string imagePath);

        AsyncTexture2D GetImage(string imageUrl);
        bool HasFinishedAchievement(int achievementId);
        bool HasFinishedAchievementBit(int achievementId, int positionIndex);

        Task LoadPlayerAchievements();
    }
}
