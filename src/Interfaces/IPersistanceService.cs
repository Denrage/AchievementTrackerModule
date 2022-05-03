using Denrage.AchievementTrackerModule.Models.Persistance;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IPersistanceService
    { 
        Storage Get();
        
        void Save(int achievementTrackWindowLocationX, int achievementTrackWindowLocationY, bool showTrackWindow);
    }
}