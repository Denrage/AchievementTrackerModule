using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{
    public class AchievementItemOverview : View
    {
        private readonly IAchievementService achievementService;
        private readonly IEnumerable<CategoryAchievements> achievements;
        private readonly IAchievementListItemFactory achievementListItemFactory;
        private readonly string title;

        public AchievementItemOverview(
            IEnumerable<CategoryAchievements> achievements,
            string title,
            IAchievementService achievementService,
            IAchievementListItemFactory achievementListItemFactory)
        {
            this.achievements = achievements;
            this.title = title;
            this.achievementService = achievementService;
            this.achievementListItemFactory = achievementListItemFactory;
        }

        protected override void Build(Container buildPanel)
        {
            var panel = new FlowPanel()
            {
                Title = this.title,
                ShowBorder = true,
                Parent = buildPanel,
                Size = buildPanel.ContentRegion.Size,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.LeftToRight,
            };

            foreach (var achievement in this.achievements
                .Select(x => (this.achievementService.HasFinishedAchievement(x.Achievement.Id), x))
                .OrderBy(x => x.Item1)
                .ThenBy(x => x.x.Category.Name)
                .ThenBy(x => x.x.Achievement.Name)
                .Select(x => x.x))
            {
                var viewContainer = new ViewContainer()
                {
                    Size = new Point(panel.Width / 2, 100),
                    ShowBorder = true,
                    Parent = panel,
                };

                viewContainer.Show(this.achievementListItemFactory.Create(achievement.Achievement, achievement.Category.Icon));
            }
        }
    }
}
