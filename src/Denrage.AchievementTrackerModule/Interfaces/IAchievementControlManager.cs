using Blish_HUD.Controls;
using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IAchievementControlManager
    {
        void ChangeParent(int achievementId, Container parent);

        bool ControlExists(int achievementId);

        Control GetControl(int achievementId);
        
        void InitializeControl(int achievementId, AchievementTableEntry achievement, AchievementTableEntryDescription description);
        
        void RemoveParent(int achievementId);
    }
}