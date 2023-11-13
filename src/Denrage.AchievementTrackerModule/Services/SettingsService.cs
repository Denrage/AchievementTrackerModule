using Blish_HUD.Settings;
using Denrage.AchievementTrackerModule.Interfaces;

namespace Denrage.AchievementTrackerModule.Services
{
    public class SettingsService : ISettingsService
    {
        public SettingEntry<bool> AutoSave { get; set; }

        public SettingEntry<bool> LimitAchievements { get; set; }
    }
}
