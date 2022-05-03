using Denrage.AchievementTrackerModule.Models.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementTextControlFactory : AchievementControlFactory<AchievementTextControl, StringDescription>
    {
        protected override AchievementTextControl CreateInternal(AchievementTableEntry achievement, StringDescription description)
            => new AchievementTextControl(achievement, description);
    }
}
