using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{

    public class AchievementTrackerView : View
    {
        private readonly IAchievementApiService achievementApiService;
        private readonly IAchievementCategoryOverviewFactory achievementCategoryOverviewFactory;
        private readonly IDictionary<int, AchievementCategory> categories;
        private readonly IDictionary<MenuItem, AchievementCategory> menuItemCategories;

        public AchievementTrackerView(IAchievementApiService achievementApiService, IAchievementCategoryOverviewFactory achievementCategoryOverviewFactory)
        {
            this.achievementApiService = achievementApiService;
            this.achievementCategoryOverviewFactory = achievementCategoryOverviewFactory;
            this.categories = achievementApiService.AchievementCategories.ToDictionary(x => x.Id, y => y);
            this.menuItemCategories = new Dictionary<MenuItem, AchievementCategory>();
        }

        protected override void Build(Container buildPanel)
        {
            var menuPanel = new Panel()
            {
                Title = "Achievements",
                ShowBorder = true,
                Size = Panel.MenuStandard.Size,
                Parent = buildPanel,
                CanScroll = true,
            };

            var menu = new Menu()
            {
                Size = menuPanel.ContentRegion.Size,
                MenuItemHeight = 40,
                CanSelect = true,
                Parent = menuPanel,
            };

            var selectedMenuItemView = new ViewContainer()
            {
                FadeView = true,
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width - menuPanel.Width, menuPanel.Height),
                Location = new Point(menuPanel.Width, 0),
            };

            foreach (var group in this.achievementApiService.AchievementGroups.OrderBy(x => x.Order))
            {
                var menuItem = menu.AddMenuItem(group.Name);
                foreach (var category in group.Categories.Select(x => this.categories[x]).OrderBy(x => x.Order))
                {
                    var innerMenuItem = new MenuItem(category.Name)
                    {
                        Parent = menuItem
                    };

                    innerMenuItem.ItemSelected += (sender, e) => selectedMenuItemView.Show(this.achievementCategoryOverviewFactory.Create(this.menuItemCategories[(MenuItem)sender]));

                    this.menuItemCategories.Add(innerMenuItem, category);

                }
            }
        }
    }
}
