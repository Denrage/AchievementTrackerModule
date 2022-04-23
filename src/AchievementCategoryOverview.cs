using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    public interface IAchievementCategoryOverviewFactory
    {
        AchievementCategoryOverview Create(AchievementCategory category);
    }

    public class AchievementCategoryOverviewFactory : IAchievementCategoryOverviewFactory
    {
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly IAchievementListItemFactory achievementListItemFactory;

        public AchievementCategoryOverviewFactory(Gw2ApiManager gw2ApiManager, IAchievementListItemFactory achievementListItemFactory)
        {
            this.gw2ApiManager = gw2ApiManager;
            this.achievementListItemFactory = achievementListItemFactory;
        }

        public AchievementCategoryOverview Create(AchievementCategory category)
            => new AchievementCategoryOverview(category, this.gw2ApiManager, this.achievementListItemFactory);
    }

    public class AchievementCategoryOverview : View
    {
        private readonly AchievementCategory category;
        private readonly Gw2ApiManager apiManager;
        private readonly IAchievementListItemFactory achievementListItemFactory;
        private IEnumerable<Achievement> achievements;

        public AchievementCategoryOverview(AchievementCategory category, Gw2ApiManager apiManager, IAchievementListItemFactory achievementListItemFactory)
        {
            this.category = category;
            this.apiManager = apiManager;
            this.achievementListItemFactory = achievementListItemFactory;
        }

        protected override void Build(Container buildPanel)
        {
            var panel = new FlowPanel()
            {
                Title = this.category.Name,
                ShowBorder = true,
                Parent = buildPanel,
                Size = buildPanel.ContentRegion.Size,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.LeftToRight,
            };

            foreach (var achievement in this.achievements)
            {
                var viewContainer = new ViewContainer()
                {
                    Size = new Point(panel.Width / 2, 100),
                    ShowBorder = true,
                    Parent = panel,
                };

                viewContainer.Show(this.achievementListItemFactory.Create(achievement));
            }
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            this.achievements = await this.apiManager.Gw2ApiClient.V2.Achievements.ManyAsync(this.category.Achievements);
            return true;
        }
    }

}
