using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;

namespace AchievementTrackerModule
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {
        public const string EnvoyArmorWikiContent = "[[Envoy Armor I: Experimental Armor]] is a [[Legendary Armor (achievements)|Legendary Armor]] [[collection]] [[achievement]] to gain the first tier of the precursor armor for the [[Legendary armor]], [[Experimental Envoy armor]]. This collection is the first step to obtain the armor.\n\n== Achievement ==\n{{achievement box}}\n\n=== Collection items ===\n{{collection table header}}\n{{collection table row | Infused Living Crystal | Acquire a [[Living Crystal]] from the [[Defeat the earth elementals|Earth Elemental]] in the [[Tangled Depths]] and use it to capture energy left over in the pillars after defeating the [[Vale Guardian]] in [[Spirit Vale]].}}\n{{collection table row | Infused Soul Mirror | Acquire a [[Soul Mirror]] from [[Burnisher Kengo]] in [[Auric Basin]] and use it to capture spectral energy from the lit ghost sconce at the exit of the Spirit Woods in [[Spirit Vale]].}}\n{{collection table row | Auric Energy Crystal | Use an [[Energy Crystal (exotic)|Energy Crystal]] from the [[Vale Guardian]] in [[Spirit Vale]] to capture Auric energy from the Inner Chamber of [[Tarir, the Forgotten City]]}}\n{{collection table row | Spirit Weave | Infuse [[Spirit Thread]]s from [[Gorseval the Multifarious]] in Spirit Vale with ley-line energy released when defeating a [[Legendary Chak Gerent|chak gerent]] near the [[Ley-Line Confluence]] in the [[Tangled Depths]]. Combine 5 [[Energized Spirit Thread]]s to create a Spirit Weave.}}\n{{collection table row | Coagulated Ectoplasm | Collect [[Ectoplasmic Residue]] from the Spirit Woods and use it to create Coagulated Ectoplasm.}}\n{{collection table row | Core of Flame | Collect a Flame Core from the chest after defeating [[Sabetha the Saboteur]].}}\n{{collection table row | Arcane Dust | Collected [[Powdered Aurillium]] from high above central [[Tarir, the Forgotten City|Tarir]] and combine it with [[Bloodstone Powder]] from [[Salvation Pass]] in the [[Forsaken Thicket]]. | class=line-top}}\n{{collection table row | Mushroom Medley | Create a Mushroom Medley by combining a [[Noxious Mushroom Cap]], [[Mushroom Emperor Gills]], an [[Orrian Truffle]], and a [[Sawgill Mushroom]].}}\n{{collection table row | Giant Beehive (trophy) | Collect a [[Giant Beehive]] from the trees high above [[Salvation Pass]].}}\n{{collection table row | Vial of Forsaken Thicket Waters | Collect a vial of water from the fountains in the [[Temple of Salvation]].}}\n{{collection table row | Spirit Quest Tonic | Create a Spirit Quest Tonic by combining [[Arcane Dust]], [[Mushroom Medley]], a [[Giant Beehive]], and a [[Vial of Forsaken Thicket Waters]].}}\n{{collection table row | Bloodstone Fragment (trophy) | Collect a Bloodstone Fragment from the chest after defeating [[Matthias Gabrel]].}}\n{{collection table row | Bloodstone Battery (Charged) | Collect a [[Bloodstone Battery (Empty)|Bloodstone Battery]] from the [[Stronghold of the Faithful]] and charge it with energy using the machinery in [[Rata Novus]]. | class=line-top}}\n{{collection table row | Soul of the Keep | Collect a [[Stone Soul]] after defeating the [[Keep Construct]] and use chak [[Goop|goop]] to free the soul inside.}}\n{{collection table row | Tormented Aurillium | Trade with [[Scavenger Rakatin]] in [[Auric Basin]] to acquire a piece of [[Polished Aurillium]] and use it to capture the tormented energies of the [[Twisted Castle]].}}\n{{collection table row | Spirit Strings | Purchase [[Itzel Spirit Poison]] from [[Jaka Itzel]] and use it while defeating the [[Keep Construct]].}}\n{{collection table row | Bloodstone-Infused Ectoplasm | Found inside the [[Twisted Castle]].}}\n{{collection table row | White Mantle Ritual Goblet | Collect a White Mantle Ritual Goblet from the chest after defeating [[Xera]].}}\n|}\n\n== Notes ==\n* [[Spirit Strings]] is acquired by opening the [[Keep Construct's Coffer]] (NOT THE CHEST) which is obtained by defeating [[Keep Construct]], despite what the achievement states. To get achievement credit for opening the coffer make sure you have [[Itzel Spirit Poison (effect)]] (buff) active when you open [[Keep Construct's Coffer]] and also be sure to open it while inside a cleared instance of the map. Note: It is possible to leave the map and return before opening the coffer. However, you would need to re-apply the [[Itzel Spirit Poison]] and be in a [[Stronghold of the Faithful]] instance in which [[Keep Construct]] is still dead.\n\n== Trivia ==\n* When a mortal dies, an [[gww:Envoy|Envoy]] will appear before the newly released soul to guide them into [[the Mists]]. All envoys were once wicked criminals in life, forced by the Oracle of the Mists to serve as soul shepherds as penance for their mortal crimes. Envoys appear to possess tremendous power, including the ability to control souls and influence the dead. [[Shiro Tagachi]] was one of the known envoys.\n\n== Related collections ==\n* [[Envoy Armor II: Refined Armor]]\n\n{{Heart of Thorns content}}\n[[Category:Achievements]]\n[[de:Erfolg/Gesandten-Rüstung I: Experimentelle Rüstung]]\n[[es:Armadura del enviado I: Armadura experimental]]\n[[fr:Armure d'émissaire I : armure expérimentale]]\n";

        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        private TabbedWindow2 achievementWindow;
        private Panel tabPanel;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {

        }

        protected override async Task LoadAsync()
        {
            var groups = await Gw2ApiManager.Gw2ApiClient.V2.Achievements.Groups.AllAsync();
            var categories = await Gw2ApiManager.Gw2ApiClient.V2.Achievements.Categories.AllAsync();
            var achievementTrackerService = new AchievementTrackerService();
            achievementTrackerService.AchievementTracked += AchievementTrackerService_AchievementTracked;
            GameService.Overlay.BlishHudWindow.AddTab("AchievementTracker", ContentsManager.GetTexture("243.png"), () => new AchievementTrackerView(groups, categories, Gw2ApiManager, this.ContentsManager, achievementTrackerService));

        }

        private void AchievementTrackerService_AchievementTracked(Achievement obj)
        {
            new AchievementTrackWindow(this.ContentsManager, obj)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2),
            }.ToggleWindow();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {

        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }

    public class AchievementTrackWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly Achievement achievement;
        private readonly Texture2D texture;

        public AchievementTrackWindow(ContentsManager contentsManager, Achievement achievement)
        {
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.texture = this.contentsManager.GetTexture("156390.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            this.Title = this.achievement.Name;
            this.ConstructWindow(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 400, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 400, 600 - 30));
            new Panel()
            {
                Parent = this,
                BackgroundColor = Microsoft.Xna.Framework.Color.White,
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                                   texture,
                                   bounds);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }

    public class AchievementTrackerService
    {
        private readonly List<Achievement> activeAchievements;

        public event Action<Achievement> AchievementTracked;

        public AchievementTrackerService()
        {
            this.activeAchievements = new List<Achievement>();
        }

        public void TrackAchievement(Achievement achievement)
        {
            this.activeAchievements.Add(achievement);
            this.AchievementTracked?.Invoke(achievement);
        }

        public void RemoveAchievement(Achievement achievement)
        {
            this.activeAchievements.Remove(achievement);
        }
    }

    public class AchievementListItem : View
    {
        private readonly AchievementTrackerService achievementTrackerService;
        private readonly Achievement achievement;

        public AchievementListItem(Achievement achievement, AchievementTrackerService achievementTrackerService)
        {
            this.achievement = achievement;
            this.achievementTrackerService = achievementTrackerService;
        }

        protected override void Build(Container buildPanel)
        {
            buildPanel.Click += (s, e) => this.achievementTrackerService.TrackAchievement(this.achievement);

            var achievementTextLabel = new Label()
            {
                Text = this.achievement.Name,
                Parent = buildPanel,
                AutoSizeHeight = true,
                Width = buildPanel.ContentRegion.Width,
            };


        }
    }

    public class AchievementCategoryOverview : View
    {
        private readonly AchievementCategory category;
        private readonly Gw2ApiManager apiManager;
        private readonly ContentsManager contentsManager;
        private readonly AchievementTrackerService achievementTrackerService;
        private IEnumerable<Achievement> achievements;

        public AchievementCategoryOverview(AchievementCategory category, Gw2ApiManager apiManager, ContentsManager contentsManager, AchievementTrackerService achievementTrackerService)
        {
            this.category = category;
            this.apiManager = apiManager;
            this.contentsManager = contentsManager;
            this.achievementTrackerService = achievementTrackerService;
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
                    Size = new Point(panel.Width / 4, 80),
                    ShowBorder = true,
                    Parent = panel,
                };

                viewContainer.Show(new AchievementListItem(achievement, this.achievementTrackerService));
            }
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            this.achievements = await this.apiManager.Gw2ApiClient.V2.Achievements.ManyAsync(this.category.Achievements);
            return true;
        }
    }

    public class AchievementTrackerView : View
    {
        private readonly IEnumerable<AchievementGroup> groups;
        private readonly IEnumerable<AchievementCategory> categories1;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly ContentsManager contentsManager;
        private readonly AchievementTrackerService achievementTrackerService;
        private readonly IDictionary<int, AchievementCategory> categories;
        private readonly IDictionary<MenuItem, AchievementCategory> menuItemCategories;

        // TODO: Make AchievementService to request groups, categories and all achievements from one category
        public AchievementTrackerView(IEnumerable<AchievementGroup> groups, IEnumerable<AchievementCategory> categories, Gw2ApiManager gw2ApiManager, ContentsManager contentsManager, AchievementTrackerService achievementTrackerService)
        {
            this.groups = groups;
            categories1 = categories;
            this.gw2ApiManager = gw2ApiManager;
            this.contentsManager = contentsManager;
            this.achievementTrackerService = achievementTrackerService;
            this.categories = categories.ToDictionary(x => x.Id, y => y);
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

            foreach (var group in groups.OrderBy(x => x.Order))
            {
                var menuItem = menu.AddMenuItem(group.Name);
                foreach (var categoryId in group.Categories)
                {
                    var category = this.categories[categoryId];
                    var innerMenuItem = new MenuItem(category.Name)
                    {
                        Parent = menuItem
                    };

                    innerMenuItem.ItemSelected += (sender, e) => selectedMenuItemView.Show(new AchievementCategoryOverview(this.menuItemCategories[(MenuItem)sender], this.gw2ApiManager, this.contentsManager, this.achievementTrackerService));

                    this.menuItemCategories.Add(innerMenuItem, category);
                }
            }
        }
    }

}
