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
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl.Http;

namespace Denrage.AchievementTrackerModule
{
    public class AchievementService
    {
        private readonly ContentsManager contentsManager;

        public IReadOnlyList<Models.Achievement.AchievementTableEntry> Achievements { get; private set; }

        public IReadOnlyList<Models.Achievement.CollectionAchievementTable> AchievementDetails { get; private set; }

        public AchievementService(ContentsManager contentsManager)
        {
            this.contentsManager = contentsManager;
        }

        public async Task LoadAsync()
        {
            var serializerOptions = new JsonSerializerOptions()
            {
                Converters = { new Models.Achievement.RewardConverter(), new Models.Achievement.AchievementTableEntryDescriptionConverter(), new Models.Achievement.CollectionAchievementTableEntryConverter() },
            };

            using (var achievements = this.contentsManager.GetFileStream("achievement_data.json"))
            {
                this.Achievements = (await System.Text.Json.JsonSerializer.DeserializeAsync<List<Models.Achievement.AchievementTableEntry>>(achievements, serializerOptions)).AsReadOnly();
            }

            using (var achievementDetails = this.contentsManager.GetFileStream("achievement_tables.json"))
            {
                this.AchievementDetails = (await System.Text.Json.JsonSerializer.DeserializeAsync<List<Models.Achievement.CollectionAchievementTable>>(achievementDetails, serializerOptions)).AsReadOnly();
            }
        }
    }

    public class AchievementTrackWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly Achievement achievement;
        private readonly Gw2ApiManager gw2ApiManager;
        private readonly AchievementService achievementService;
        private readonly Texture2D texture;
        private FlowPanel panel;
        private IEnumerable<AccountAchievement> playerAchievements;

        public AchievementTrackWindow(ContentsManager contentsManager, Achievement achievement, Gw2ApiManager gw2ApiManager, AchievementService achievementService)
        {
            this.contentsManager = contentsManager;
            this.achievement = achievement;
            this.gw2ApiManager = gw2ApiManager;
            this.achievementService = achievementService;
            texture = this.contentsManager.GetTexture("156390.png");
            BuildWindow();
        }

        private void BuildWindow()
        {
            Title = achievement.Name;
            ConstructWindow(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 7 * 74, 600), new Microsoft.Xna.Framework.Rectangle(0, 30, 7 * 74, 600 - 30));
            panel = new FlowPanel()
            {
                Parent = this,
                Size = ContentRegion.Size,
                CanScroll = true,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5f),
            };
        }

        protected override void OnShown(EventArgs e)
        {
            if (playerAchievements == null && gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
            {
                playerAchievements = gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().Result;
            }

            Task.Run(async () =>
            {
                try
                {
                    var items = this.achievementService.AchievementDetails.FirstOrDefault(x => x.Id == achievement.Id).Entries.SelectMany(x => x).OfType<Models.Achievement.CollectionAchievementTable.CollectionAchievementTableItemEntry>();

                    foreach (var item in items)
                    {
                        var tint = playerAchievements == null || playerAchievements.FirstOrDefault(x => x.Id == achievement.Id) == null;
                        var imageStream = await ("https://wiki.guildwars2.com" + item.ImageUrl).WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36").GetStreamAsync();


                        var texture = new Blish_HUD.Content.AsyncTexture2D(ContentService.Textures.TransparentPixel);

                        GameService.Graphics.QueueMainThreadRender(_ =>
                        {
                            texture.SwapTexture(TextureUtil.FromStreamPremultiplied(imageStream));
                            imageStream.Close();
                        });

                        var image = new Image()
                        {
                            Parent = panel,
                            Width = 64,
                            Height = 64,
                            Texture = texture,
                        };

                        if (false && tint)
                        {
                            image.Tint = Microsoft.Xna.Framework.Color.Gray;
                        }

                        image.Click += (s, eventArgs) =>
                        {
                            //var itemWindow = itemDetailWindowFactory.Create(item);
                            //itemWindow.Parent = GameService.Graphics.SpriteScreen;
                            //itemWindow.Location = GameService.Graphics.SpriteScreen.Size / new Point(2) - new Point(256, 178) / new Point(2);
                            //itemWindow.ToggleWindow();
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
