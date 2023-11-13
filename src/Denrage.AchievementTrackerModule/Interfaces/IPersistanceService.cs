using Denrage.AchievementTrackerModule.Models.Persistance;
using System;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IPersistanceService
    {
        event Action AutoSave;

        Storage Get();
        void RegisterSaveDelegate(Action<Storage> action);
        void Save(int achievementTrackWindowLocationX, int achievementTrackWindowLocationY, bool showTrackWindow);
    }
}