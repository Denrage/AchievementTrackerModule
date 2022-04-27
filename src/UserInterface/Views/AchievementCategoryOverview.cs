using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{
    public class AchievementCategoryOverview : View
    {
        private readonly AchievementCategory category;
        private readonly Gw2ApiManager apiManager;
        private readonly IAchievementListItemFactory achievementListItemFactory;
        private readonly IAchievementService achievementService;
        private readonly List<(int AchievementId, ViewContainer Card)> achievementCards;
        private IEnumerable<Achievement> achievements;

        public AchievementCategoryOverview(AchievementCategory category, Gw2ApiManager apiManager, IAchievementListItemFactory achievementListItemFactory, IAchievementService achievementService)
        {
            this.category = category;
            this.apiManager = apiManager;
            this.achievementListItemFactory = achievementListItemFactory;
            this.achievementService = achievementService;
            this.achievementCards = new List<(int AchievementId, ViewContainer Card)>();

            this.achievementService.PlayerAchievementsLoaded += this.AchievementService_PlayerAchievementsLoaded;
        }

        private void AchievementService_PlayerAchievementsLoaded()
        {
            var itemsToRemove = new List<(int AchievementId, ViewContainer Card)>();
            foreach (var item in this.achievementCards)
            {
                if (this.achievementService.HasFinishedAchievement(item.AchievementId))
                {
                    item.Card.Dispose();
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                _ = this.achievementCards.Remove(item);
            }
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

            foreach (var achievement in this.achievements.Select(x => (this.achievementService.HasFinishedAchievement(x.Id), x)).OrderBy(x => x.Item1).ThenBy(x => x.x.Name).Select(x => x.x))
            {
                var viewContainer = new ViewContainer()
                {
                    Size = new Point(panel.Width / 2, 100),
                    ShowBorder = true,
                    Parent = panel,
                };

                viewContainer.Show(this.achievementListItemFactory.Create(achievement, this.category.Icon));
            }
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            this.achievements = await this.apiManager.Gw2ApiClient.V2.Achievements.ManyAsync(this.category.Achievements);
            return true;
        }
    }
}
