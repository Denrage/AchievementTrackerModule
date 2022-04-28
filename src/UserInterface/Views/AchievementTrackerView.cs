using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.Models.Achievement;
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
        private readonly IDictionary<int, AchievementCategory> categories;
        private readonly IDictionary<MenuItem, AchievementCategory> menuItemCategories;

        private ViewContainer selectedMenuItemView;

        private Task delayTask;
        private CancellationTokenSource delayCancellationToken;
        private TextBox searchBar;

        public AchievementTrackerView(IAchievementItemOverviewFactory achievementItemOverviewFactory, IAchievementService achievementService)
        {
            this.achievementItemOverviewFactory = achievementItemOverviewFactory;
            this.achievementService = achievementService;
            this.categories = achievementService.AchievementCategories.ToDictionary(x => x.Id, y => y);
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

            this.searchBar.TextChanged += this.SearchBar_TextChanged;

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

            var menu = new Menu()
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

            foreach (var group in this.achievementService.AchievementGroups.OrderBy(x => x.Order))
            {
                var menuItem = menu.AddMenuItem(group.Name);
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
                        this.selectedMenuItemView.Show(
                            this.achievementItemOverviewFactory.Create(
                                achievements.Select(x => (menuItemCategory, x)),
                                menuItemCategory.Name));
                    };

                    this.menuItemCategories.Add(innerMenuItem, category);

                }
            }
        }

        private void SearchBar_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                if (this.delayTask != null)
                {
                    this.delayCancellationToken.Cancel();
                    this.delayTask = null;
                    this.delayCancellationToken = null;
                }

                this.delayCancellationToken = new CancellationTokenSource();
                this.delayTask = new Task(async () => await this.DelaySeach(this.delayCancellationToken.Token), this.delayCancellationToken.Token);
                this.delayTask.Start();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DelaySeach(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(300, cancellationToken);
                await this.SearchAsync(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        private Dictionary<AchievementCategory, IEnumerable<AchievementTableEntry>> achievementCache;

        private async Task SearchAsync(CancellationToken cancellationToken = default)
        {
            await this.searchSemaphore.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var searchText = this.searchBar.Text;

                if (this.achievementCache is null)
                {
                    var achievements = new Dictionary<AchievementCategory, IEnumerable<AchievementTableEntry>>();

                    foreach (var item in this.categories.Values)
                    {
                        if (item.Achievements.Count > 0)
                        {
                            achievements[item] = this.achievementService.Achievements.Where(x => item.Achievements.Contains(x.Id));
                        }
                    }

                    this.achievementCache = achievements;
                }

                var searchedAchievements = new List<(AchievementCategory, AchievementTableEntry)>();

                foreach (var item in this.achievementCache)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var categoryAchievement in item.Value.Where(x => x.Name.ToUpper().Contains(searchText.ToUpper())))
                    {
                        searchedAchievements.Add((item.Key, categoryAchievement));
                    }
                }

                this.selectedMenuItemView.Show(this.achievementItemOverviewFactory.Create(searchedAchievements, searchText));
            }
            finally
            {
                _ = this.searchSemaphore.Release();
            }
        }
    }
}
