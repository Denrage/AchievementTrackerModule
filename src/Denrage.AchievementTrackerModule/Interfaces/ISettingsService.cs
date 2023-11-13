using Blish_HUD.Settings;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface ISettingsService
    {
        SettingEntry<bool> AutoSave { get; set; }

        SettingEntry<bool> LimitAchievements { get; set; }
    }
}
