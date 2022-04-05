using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.Net;
using System.Threading.Tasks;

namespace AchievementTrackerModule
{
    public class CollectionItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Acquisition { get; set; }

        public string IconUrl { get; set; }
    }

    public class WikiParserService
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36";
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly WebClient wikiDownloadWebClient;


        public WikiParserService(Gw2ApiManager gw2ApiManager)
        {
            this.gw2ApiManager = gw2ApiManager;
            this.wikiDownloadWebClient = new WebClient();
        }

        public async Task<IEnumerable<CollectionItem>> ParseWikiCollectionTableAsync(string name)
        {
            var wikiSource = await this.GetWikiContentAsync(name);
            var result = new List<CollectionItem>();
            var collectionLines = wikiSource.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Contains("{{collection"));

            foreach (var row in collectionLines)
            {
                if (row.Contains("collection table header"))
                {
                    continue;
                }

                result.Add(await this.ParseCollectionRowAsync(row));
            }

            return result;
        }

        public async Task<CollectionItem> ParseCollectionRowAsync(string row)
        {
            row = row.Replace("{{", string.Empty).Replace("}}", string.Empty); // {{collection table row | ... }} -> collection table row | ...
            var parts = row.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray(); // convert columns to array
            var collectionItem = parts[0].Trim();
            var collectionAcquisitionText = string.Join("|", parts.Skip(1)).Trim();

            var item = await this.GetIdFromNameAsync(collectionItem);
            var apiItem = await this.gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(item);


            return new CollectionItem()
            {
                Acquisition = collectionAcquisitionText,
                IconUrl = apiItem.Icon.ToString(),
                Id = apiItem.Id,
                Name = apiItem.Name,
            };
        }

        private string CreateWikiApiUrl(string title)
            => $"https://wiki.guildwars2.com/api.php?action=query&prop=revisions&titles={WebUtility.UrlEncode(title)}&rvslots=*&rvprop=content&formatversion=2&format=json";

        private async Task<string> GetWikiContentAsync(string name)
        {
            try
            {
                var requestTitle = CreateWikiApiUrl(name);
                this.wikiDownloadWebClient.Headers.Add("user-agent", USER_AGENT);
                var wikiResult = await this.wikiDownloadWebClient.DownloadStringTaskAsync(requestTitle);
                var parsedWikiResult = System.Text.Json.JsonSerializer.Deserialize<Root>(wikiResult);
                return parsedWikiResult.query.pages[0].revisions[0].slots.main.content;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> GetIdFromNameAsync(string name)
        {
            var wikiContent = await this.GetWikiContentAsync(name);
            var idFirstIndex = wikiContent.IndexOf("| id") + "| id = ".Length;
            var idLastIndex = wikiContent.IndexOf("\n", idFirstIndex);
            var result = wikiContent.Substring(idFirstIndex, idLastIndex - idFirstIndex);
            return int.Parse(result);
        }

        public class Main
        {
            public string contentmodel { get; set; }
            public string contentformat { get; set; }
            public string content { get; set; }
        }

        public class Slots
        {
            public Main main { get; set; }
        }

        public class RevisionsItem
        {
            public Slots slots { get; set; }
        }

        public class PagesItem
        {
            public int pageid { get; set; }
            public int ns { get; set; }
            public string title { get; set; }
            public List<RevisionsItem> revisions { get; set; }
        }

        public class Query
        {
            public List<PagesItem> pages { get; set; }
        }

        public class Root
        {
            public bool batchcomplete { get; set; }
            public Query query { get; set; }
        }
    }


    public class AchievementTrackWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly Achievement achievement;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly WikiParserService wikiParserService;
        private readonly IItemDetailWindowFactory itemDetailWindowFactory;
        private readonly Texture2D texture;
        private FlowPanel panel;
        private IEnumerable<AccountAchievement> playerAchievements;

        public AchievementTrackWindow(ContentsManager contentsManager, Achievement achievement, Gw2ApiManager gw2ApiManager, WikiParserService wikiParserService, IItemDetailWindowFactory itemDetailWindowFactory)
        {
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.gw2ApiManager = gw2ApiManager;
            this.wikiParserService = wikiParserService;
            this.itemDetailWindowFactory = itemDetailWindowFactory;
            this.texture = this.contentsManager.GetTexture("156390.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            this.Title = this.achievement.Name;
            this.ConstructWindow(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 7 * 74, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 7 * 74, 600 - 30));
            this.panel = new FlowPanel()
            {
                Parent = this,
                Size = this.ContentRegion.Size,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5f),
            };
        }

        protected override void OnShown(EventArgs e)
        {
            if (this.playerAchievements == null && this.gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
            {
                this.playerAchievements = this.gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().Result;
            }

            Task.Run(async () =>
            {
                try
                {
                    var items = await this.wikiParserService.ParseWikiCollectionTableAsync(this.achievement.Name);

                    foreach (var item in items)
                    {
                        var tint = this.playerAchievements == null || this.playerAchievements.FirstOrDefault(x => x.Id == this.achievement.Id) == null;
                        var image = new Image()
                        {
                            Parent = this.panel,
                            Width = 64,
                            Height = 64,
                            Texture = Content.GetRenderServiceTexture(item.IconUrl),
                            BackgroundColor = Microsoft.Xna.Framework.Color.Gray,
                        };

                        if (tint)
                        {
                            image.Tint = Microsoft.Xna.Framework.Color.Gray;
                        }

                        image.Click += (s, eventArgs) =>
                        {
                            var itemWindow = this.itemDetailWindowFactory.Create(item);
                            itemWindow.Parent = GameService.Graphics.SpriteScreen;
                            itemWindow.Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2);
                            itemWindow.ToggleWindow();
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            });

            base.OnShown(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                                   texture,
                                   bounds);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }

}
