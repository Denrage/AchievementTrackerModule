using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Gw2Sharp.WebApi.V2.Models;

namespace Denrage.AchievementTrackerModule.Models
{
    public class CategoryAchievements
    {
        public CategoryAchievements(AchievementCategory category, AchievementTableEntry achievement)
        {
            this.Category = category;
            this.Achievement = achievement;
        }

        public AchievementCategory Category { get; set; }
        public AchievementTableEntry Achievement { get; set; }

    }
}
