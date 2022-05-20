using Denrage.AchievementTrackerModule.UserInterface.Controls.FormattedLabel;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IFormattedLabelHtmlService
    {
        FormattedLabelBuilder CreateLabel(string textWithHtml);
    }
}