using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class AchievementButton : DetailsButton
    {
        public string Description { get; set; }

        public AsyncTexture2D AchievementIcon { get; set; }

        public string CompleteText { get; set; }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if(this.Description != null)
            {
                spriteBatch.DrawStringOnCtrl(this,
                   this.Description,
                   Control.Content.DefaultFont14,
                   new Rectangle(_size.Y + 20, 5, _size.X - _size.Y - 35, base.Height),
                   Color.LightGreen,
                   wrap: true,
                   stroke: true);
            }

            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.CompleteText != null)
            {
                spriteBatch.DrawStringOnCtrl(this,
                   this.CompleteText,
                   Control.Content.DefaultFont14,
                   new Rectangle(_size.Y + 20, -20, _size.X - _size.Y - 35, base.Height),
                   Color.White,
                   wrap: true,
                   stroke: true);
            }
        }
    }
}
