using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Models.Achievement;
using Flurl.Http;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Denrage.AchievementTrackerModule.Models.Achievement.CollectionAchievementTable;

namespace Denrage.AchievementTrackerModule
{
    public interface IItemDetailWindowFactory
    {
        ItemDetailWindow Create(string name, string[] columns, List<CollectionAchievementTableEntry> item);
    }

    public class ItemDetailWindowFactory : IItemDetailWindowFactory
    {
        private readonly ContentsManager contentsManager;
        private readonly AchievementService achievementService;

        public ItemDetailWindowFactory(ContentsManager contentsManager, AchievementService achievementService)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
        }

        public ItemDetailWindow Create(string name, string[] columns, List<CollectionAchievementTableEntry> item)
            => new ItemDetailWindow(this.contentsManager, this.achievementService, name, columns, item);
    }

    public class ItemDetailWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly AchievementService achievementService;
        private readonly string name;
        private readonly string[] columns;
        private readonly List<CollectionAchievementTableEntry> item;
        private readonly Texture2D texture;

        public ItemDetailWindow(ContentsManager contentsManager, AchievementService achievementService, string name, string[] columns, List<CollectionAchievementTableEntry> item)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.name = name;
            this.columns = columns;
            this.item = item;
            texture = this.contentsManager.GetTexture("156390.png");
            BuildWindow();

        }

        private void BuildWindow()
        {
            Title = this.name;
            ConstructWindow(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 400), new Microsoft.Xna.Framework.Rectangle(0, 30, 300, 400 - 30));

            var panel = new FlowPanel()
            {
                Parent = this,
                Size = this.ContentRegion.Size,
                FlowDirection = ControlFlowDirection.TopToBottom,
            };

            for (int i = 0; i < this.item.Count; i++)
            {
                var innerPannel = new Panel()
                {
                    Parent = panel,
                    Width = panel.ContentRegion.Width,
                    HeightSizingMode = SizingMode.AutoSize,
                };

                var label = new Label()
                {
                    Parent = innerPannel,
                    Width = (int)System.Math.Floor(0.3 * innerPannel.ContentRegion.Width),
                    Text = this.columns[i],
                };

                Control control = null;

                switch (this.item[i])
                {
                    case CollectionAchievementTable.CollectionAchievementTableMapEntry mapEntry:
                        control = new Image()
                        {
                            Texture = this.achievementService.GetDirectImageLink(mapEntry.ImageLink),
                            Width = 120,
                            Height = 120,
                        };
                        break;
                    case CollectionAchievementTable.CollectionAchievementTableCoinEntry coinEntry:
                        control = new Label()
                        {
                            Text = coinEntry.ItemId + ": " + coinEntry.Type.ToString(),
                            AutoSizeHeight = true,
                            WrapText = true,
                        };
                        break;
                    case CollectionAchievementTable.CollectionAchievementTableItemEntry itemEntry:
                        control = new Label()
                        {
                            Text = itemEntry.Name,
                            AutoSizeHeight = true,
                            WrapText = true,
                        };
                        break;
                    case CollectionAchievementTable.CollectionAchievementTableLinkEntry linkEntry:
                        control = new Label()
                        {
                            Text = linkEntry.Text,
                            AutoSizeHeight = true,
                            WrapText = true,
                        };
                        break;
                    case CollectionAchievementTable.CollectionAchievementTableNumberEntry numberEntry:
                        control = new Label()
                        {
                            Text = numberEntry.Number.ToString(),
                            AutoSizeHeight = true,
                            WrapText = true,
                        };
                        break;
                    case CollectionAchievementTable.CollectionAchievementTableStringEntry stringEntry:
                        control = new Label()
                        {
                            Text = stringEntry.Text,
                            AutoSizeHeight = true,
                            WrapText = true,
                        };
                        break;
                    default:
                        break;
                }

                if (control != null)
                {
                    control.Parent = innerPannel;
                    control.Width = innerPannel.Width - label.Width;
                    control.Location = new Microsoft.Xna.Framework.Point(label.Width, 0);
                }
            }
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
