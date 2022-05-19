// See https://aka.ms/new-console-template for more information

using Denrage.AchievementTrackerModule.Libs.Interfaces;
using System.Collections.Generic;

namespace Denrage.AchievementTrackerModule.Libs.Achievement
{
    public class ItemSubPageInformation : SubPageInformation, IHasDescriptionList, IHasInteractiveMap, IHasImage, IHasAdditionalImages
    {
        public string ImageUrl { get; set; }

        public List<KeyValuePair<string, string>> DescriptionList { get; set; } = new List<KeyValuePair<string, string>>();

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public InteractiveMapInformation InteractiveMap { get; set; }

        public string Acquisition { get; set; }
    }
}