using Denrage.AchievementTrackerModule.Libs.Interfaces;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    public class QuestSubPageInformation : SubPageInformation, IHasDescriptionList, IHasInteractiveMap
    {
        public string ImageUrl { get; set; }

        public List<KeyValuePair<string, string>> DescriptionList { get; set; } = new List<KeyValuePair<string, string>>();

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public InteractiveMapInformation InteractiveMap { get; set; }
    }
}