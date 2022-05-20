using Denrage.AchievementTrackerModule.Libs.Achievement;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface ISubPageInformationWindowManager
    {
        void CloseWindows();

        void Create(SubPageInformation subPageInformation);
    }
}
