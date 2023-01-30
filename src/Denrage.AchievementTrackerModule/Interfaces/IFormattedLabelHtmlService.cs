using Blish_HUD.Controls;

namespace Denrage.AchievementTrackerModule.Interfaces
{
    public interface IFormattedLabelHtmlService
    {
        FormattedLabelBuilder CreateLabel(string textWithHtml);
    }
}