using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Views
{

    public class AchievementTrackerView : View
    {
        private readonly SemaphoreSlim searchSemaphore = new SemaphoreSlim(1, 1);

        private readonly IAchievementItemOverviewFactory achievementItemOverviewFactory;
        private readonly IAchievementService achievementService;
        private readonly IDictionary<MenuItem, AchievementCategory> menuItemCategories;
        private Menu menu;
        private ViewContainer selectedMenuItemView;

        private Task delayTask;
        private CancellationTokenSource delayCancellationToken;
        private TextBox searchBar;

        public AchievementTrackerView(IAchievementItemOverviewFactory achievementItemOverviewFactory, IAchievementService achievementService)
        {
            this.achievementItemOverviewFactory = achievementItemOverviewFactory;
            this.achievementService = achievementService;
            this.menuItemCategories = new Dictionary<MenuItem, AchievementCategory>();
        }

        protected override void Build(Container buildPanel)
        {
            this.searchBar = new TextBox()
            {
                // TODO: Localization
                PlaceholderText = "Search...",
                Width = Panel.MenuStandard.Size.X,
                Parent = buildPanel,
            };

            this.searchBar.TextChanged += (object sender, EventArgs e) =>
            {
                if (!this.searchBar.Enabled) return;

                Search(this.searchBar.Text);
            };

            var menuPanel = new Panel()
            {
                Title = "Achievements",
                ShowBorder = true,
                Width = Panel.MenuStandard.Size.X,
                Height = Panel.MenuStandard.Size.Y - this.searchBar.Height - 10,
                Location = new Point(0, this.searchBar.Height + 10),
                Parent = buildPanel,
                CanScroll = true,
            };

            this.menu = new Menu()
            {
                Size = menuPanel.ContentRegion.Size,
                MenuItemHeight = 40,
                CanSelect = true,
                Parent = menuPanel,
            };

            this.selectedMenuItemView = new ViewContainer()
            {
                FadeView = true,
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width - menuPanel.Width, menuPanel.Height),
                Location = new Point(menuPanel.Width, 0),
            };

            var apiErrorLabel = new Label()
            {
                // TODO: Localization
                Text = "Weren't able to gather needed information from the API or it is still ongoing. Consult the log for details. Retrying every 5 minutes",
                Parent = buildPanel,
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                Width = 250,
                WrapText = true,
                TextColor = Microsoft.Xna.Framework.Color.Red,
            };

            apiErrorLabel.Visible = false;

            apiErrorLabel.Location = new Point(((this.selectedMenuItemView.Width - apiErrorLabel.Width) / 2) + this.selectedMenuItemView.Location.X, ((this.selectedMenuItemView.Height - apiErrorLabel.Height) / 2) + this.selectedMenuItemView.Location.Y);

            if (this.achievementService.AchievementGroups is null || this.achievementService.AchievementCategories is null)
            {
                this.achievementService.ApiAchievementsLoaded += () =>
                {
                    apiErrorLabel.Visible = false;
                    this.searchBar.Enabled = true;
                    this.InitializeAchievementElements();
                };

                this.searchBar.Enabled = false;
                apiErrorLabel.Visible = true;
            }
            else
            {
                this.InitializeAchievementElements();
            }
        }

        private void InitializeAchievementElements()
        {
            this.categories = this.achievementService.AchievementCategories.ToDictionary(x => x.Id, y => y);
            foreach (var group in this.achievementService.AchievementGroups.OrderBy(x => x.Order))
            {
                var menuItem = this.menu.AddMenuItem(group.Name);
                foreach (var category in group.Categories.Select(x => this.categories[x]).OrderBy(x => x.Order))
                {
                    var innerMenuItem = new MenuItem(category.Name)
                    {
                        Parent = menuItem
                    };

                    innerMenuItem.ItemSelected += (sender, e) =>
                    {
                        var menuItemCategory = this.menuItemCategories[(MenuItem)sender];
                        var achievements = this.achievementService.Achievements.Where(x => menuItemCategory.Achievements.Contains(x.Id));
                        this.selectedMenuItemView.Clear();
                        this.selectedMenuItemView.Show(
                            this.achievementItemOverviewFactory.Create(
                                achievements.Select(x => new CategoryAchievements(menuItemCategory, x)),
                                menuItemCategory.Name));
                    };

                    this.menuItemCategories.Add(innerMenuItem, category);

                }
            }
        }

        private Dictionary<AchievementCategory, IEnumerable<AchievementTableEntry>> achievementCache;
        private Dictionary<int, AchievementCategory> categories;

        private void Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                this.selectedMenuItemView.Clear();
                return;
            }

            //Search
            var achievements = achievementService.Achievements.Where(a => a.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(50)
                .Select(a => new CategoryAchievements(
                    achievementService.AchievementCategories.FirstOrDefault(x => x.Achievements.Contains(a.Id)),
                    a))
                .Where(a => a.Category != null);

            this.selectedMenuItemView.Clear();
            this.selectedMenuItemView.Show(this.achievementItemOverviewFactory.Create(achievements, searchText));
        }
    }
}
