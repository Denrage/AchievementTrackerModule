using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class ImageSpinner : Panel
    {
        private readonly Point defaultLoadingSpinnerSize;
        private readonly Image image;
        private readonly LoadingSpinner loadingSpinner;

        public ImageSpinner(AsyncTexture2D texture)
        {
            texture.TextureSwapped += (s, e) =>
            {
                this.loadingSpinner.Hide();
                this.image.Show();
            };

            this.image = new Image()
            {
                Parent = this,
                Visible = false,
                Texture = texture,
                Width = this.Width,
                Height = this.Height,
            };

            this.loadingSpinner = new LoadingSpinner()
            {
                Parent = this,
                Visible = true,
            };

            this.defaultLoadingSpinnerSize = this.loadingSpinner.Size;

            this.loadingSpinner.Size = new Microsoft.Xna.Framework.Point(Math.Min(this.Width, this.defaultLoadingSpinnerSize.X), Math.Min(this.Height, this.defaultLoadingSpinnerSize.Y));
            this.loadingSpinner.Location = new Microsoft.Xna.Framework.Point((this.Width / 2) - (this.loadingSpinner.Width / 2), (this.Height / 2) - (this.loadingSpinner.Height / 2));

            if (texture.Texture != ContentService.Textures.TransparentPixel)
            {
                this.image.Show();
                this.loadingSpinner.Hide();
            }
        }

        public override void RecalculateLayout()
        {
            if (this.loadingSpinner != null)
            {
                this.loadingSpinner.Size = new Microsoft.Xna.Framework.Point(Math.Min(this.Width, this.defaultLoadingSpinnerSize.X), Math.Min(this.Height, this.defaultLoadingSpinnerSize.Y));
                this.loadingSpinner.Location = new Microsoft.Xna.Framework.Point((this.Width / 2) - (this.loadingSpinner.Width / 2), (this.Height / 2) - (this.loadingSpinner.Height / 2));
            }

            if (this.image != null)
            {
                this.image.Width = this.Width;
                this.image.Height = this.Height;
            }
        }

        protected override void DisposeControl()
        {
            this.image.Dispose();
            this.loadingSpinner.Dispose();
            base.DisposeControl();
        }
    }
}
