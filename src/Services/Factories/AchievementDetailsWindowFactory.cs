using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services.Factories
{
    public class AchievementDetailsWindowFactory : IAchievementDetailsWindowFactory
    {
        private readonly ContentsManager contentsManager;
        private readonly IAchievementService achievementService;
        private readonly IAchievementControlProvider achievementControlProvider;

        public AchievementDetailsWindowFactory(ContentsManager contentsManager, IAchievementService achievementService, IAchievementControlProvider achievementControlProvider)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.achievementControlProvider = achievementControlProvider;
        }

        public AchievementDetailsWindow Create(AchievementTableEntry achievement)
            => new AchievementDetailsWindow(this.contentsManager, achievement, this.achievementService, this.achievementControlProvider);
    }
}
