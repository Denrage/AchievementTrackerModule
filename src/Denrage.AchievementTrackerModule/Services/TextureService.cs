using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;

namespace Denrage.AchievementTrackerModule.Services
{
    public class TextureService : ITextureService, IDisposable
    {
        private readonly ConcurrentDictionary<string, AsyncTexture2D> textures;
        private readonly ConcurrentDictionary<string, AsyncTexture2D> refTextures;
        private readonly ContentService contentService;
        private readonly ContentsManager contentsManager;

        public TextureService(ContentService contentService, ContentsManager contentsManager)
        {
            this.textures = new ConcurrentDictionary<string, AsyncTexture2D>();
            this.refTextures = new ConcurrentDictionary<string, AsyncTexture2D>();

            this.contentService = contentService;
            this.contentsManager = contentsManager;
        }

        public AsyncTexture2D GetTexture(string url)
        {
            var texture = this.textures.FirstOrDefault(t => t.Key.Equals(url)).Value;

            if (texture != null)
            {
                return texture;
            }

            texture = this.contentService.GetRenderServiceTexture(url);

            if (texture != null)
            {
                _ = this.textures.AddOrUpdate(url, texture, (key, value) => value = texture);
            }

            return texture;
        }

        public AsyncTexture2D GetRefTexture(string file)
        {
            var texture = this.refTextures.FirstOrDefault(t => t.Key.Equals(file)).Value;

            if (texture != null)
            {
                return texture;
            }

            texture = this.contentsManager.GetTexture(file);

            if (texture != null)
            {
                _ = this.refTextures.AddOrUpdate(file, texture, (key, value) => value = texture);
            }

            return texture;
        }

        public void Dispose()
        {
            foreach (var item in this.textures)
            {
                item.Value.Dispose();
            }

            foreach (var item in this.refTextures)
            {
                item.Value.Dispose();
            }

            this.textures.Clear();
            this.refTextures.Clear();
        }
    }
}
