using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;

namespace AchievementTrackerModule
{
    public interface IItemDetailWindowFactory
    {
        ItemDetailWindow Create(CollectionItem item);
    }

    public class ItemDetailWindowFactory : IItemDetailWindowFactory
    {
        private readonly ContentsManager contentsManager;

        public ItemDetailWindowFactory(ContentsManager contentsManager)
        {
            this.contentsManager = contentsManager;
        }

        public ItemDetailWindow Create(CollectionItem item)
            => new ItemDetailWindow(this.contentsManager, item);
    }

    public class ItemDetailWindow : WindowBase2
    {
        private readonly ContentsManager contentsManager;
        private readonly CollectionItem item;
        private readonly Texture2D texture;

        public ItemDetailWindow(ContentsManager contentsManager, CollectionItem item)
        {
            this.contentsManager = contentsManager;
            this.item = item;
            this.texture = this.contentsManager.GetTexture("156390.png");
            this.BuildWindow();
        }

        private void BuildWindow()
        {
            this.Title = this.item.Name;
            this.ConstructWindow(texture, new Microsoft.Xna.Framework.Rectangle(0, 0, 300, 400), new Microsoft.Xna.Framework.Rectangle(0, 30, 300, 400 - 30));

            var panel = new Panel()
            {
                Size = this.ContentRegion.Size,
                Parent = this,
            };

            new Label()
            {
                Text = this.item.Acquisition,
                Size = panel.ContentRegion.Size,
                WrapText = true,
                Parent = panel,
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

}
