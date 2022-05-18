using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Libs.Interfaces
{
    public interface IHasDescriptionList
    {
        List<KeyValuePair<string, string>> DescriptionList { get; set; }
    }
}