using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.UserInterface.Controls;

namespace Denrage.AchievementTrackerModule.Services.Factories.AchievementControl
{
    public class AchievementTextControlFactory : AchievementControlFactory<AchievementTextControl, StringDescription>
    {
        private readonly IFormattedLabelHtmlService formattedLabelHtmlService;

        public AchievementTextControlFactory(IFormattedLabelHtmlService formattedLabelHtmlService)
        {
            this.formattedLabelHtmlService = formattedLabelHtmlService;
        }

        protected override AchievementTextControl CreateInternal(AchievementTableEntry achievement, StringDescription description)
            => new AchievementTextControl(this.formattedLabelHtmlService, achievement, description);
    }
}
