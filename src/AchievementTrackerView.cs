using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule
{
    public interface IAchievementApiService
    {
        IEnumerable<AchievementGroup> AchievementGroups { get; }

        IEnumerable<AchievementCategory> AchievementCategories { get; }
    }

    public class AchievementApiService : IAchievementApiService
    {
        private readonly Gw2ApiManager gw2ApiManager;

        public IEnumerable<AchievementGroup> AchievementGroups { get; private set; }

        public IEnumerable<AchievementCategory> AchievementCategories { get; private set; }

        public AchievementApiService(Gw2ApiManager gw2ApiManager)
        {
            this.gw2ApiManager = gw2ApiManager;
        }

        public async Task LoadAsync()
        {
            this.AchievementGroups = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.Groups.AllAsync();
            this.AchievementCategories = await this.gw2ApiManager.Gw2ApiClient.V2.Achievements.Categories.AllAsync();
        }
    }

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
                foreach (var categoryId in group.Categories)
                {
                    var category = this.categories[categoryId];

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
