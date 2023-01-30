using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    public class ImageSpinner : Panel
    {
        private readonly Point defaultLoadingSpinnerSize;
        private readonly LoadingSpinner loadingSpinner;
        
        public Image Image { get; private set; }

        public Color Tint
        {
            get => this.Image.Tint;
            set => this.Image.Tint = value;
        }

        public ImageSpinner(AsyncTexture2D texture)
        {
            texture.TextureSwapped += (s, e) =>
            {
                this.loadingSpinner.Hide();
                this.Image.Show();
            };

            this.Image = new Image()
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
                this.Image.Show();
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

            if (this.Image != null)
            {
                this.Image.Width = this.Width;
                this.Image.Height = this.Height;
            }
        }

        protected override void DisposeControl()
        {
            this.Image.Dispose();
            this.loadingSpinner.Dispose();
            base.DisposeControl();
        }
    }
}
