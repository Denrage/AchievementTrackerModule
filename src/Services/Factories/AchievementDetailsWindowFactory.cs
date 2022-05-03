using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementDetailsWindowFactory : IAchievementDetailsWindowFactory
    {
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementControlProvider achievementControlProvider;
        private readonly IAchievementControlManager achievementControlManager;

        public AchievementDetailsWindowFactory(ContentsManager contentsManager, IAchievementService achievementService, IAchievementControlProvider achievementControlProvider, IAchievementControlManager achievementControlManager)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementControlProvider = achievementControlProvider;
            this.achievementControlManager = achievementControlManager;
        }

        public AchievementDetailsWindow Create(AchievementTableEntry achievement)
            => new AchievementDetailsWindow(this.contentsManager, achievement, this.achievementService, this.achievementControlManager);
    }
}
