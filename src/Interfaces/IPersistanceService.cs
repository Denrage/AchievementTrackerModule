using Denrage.AchievementTrackerModule.Services;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IPersistanceService
    {
        void AddAchievementWindowInformation(int achievementId, int positionX, int positionY);
        
        void AddItemInformation(int achievementId, int itemIndex, string name, int positionX, int positionY);
        
        Storage Get();
        
        void Save();
        
        void TrackAchievement(int achievementId);
    }
}