using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;

namespace Denrage.AchievementTrackerModule.Services
{
    public interface ITextureService
    {
        AsyncTexture2D GetTexture(string url);

        AsyncTexture2D GetRefTexture(string fileName);
    }
    public class TextureService : ITextureService
    {
        private ContentService contentService { get;  }

        private ContentsManager contentsManager { get; }

        private ConcurrentDictionary<string, AsyncTexture2D> Textures { get; set; }
        private ConcurrentDictionary<string, AsyncTexture2D> RefTextures { get; set; }

        public TextureService(ContentService contentService, ContentsManager contentsManager)
        {
            Textures = new ConcurrentDictionary<string, AsyncTexture2D>();
            RefTextures = new ConcurrentDictionary<string, AsyncTexture2D>();

            this.contentService = contentService;
            this.contentsManager = contentsManager;
        }

        public AsyncTexture2D GetTexture(string url)
        {
            var texture = Textures.FirstOrDefault(t => t.Key.Equals(url)).Value;

            if (texture != null)
                return texture;

            texture = contentService.GetRenderServiceTexture(url);

            if(texture != null)
                Textures.AddOrUpdate(url, texture, (key, value) => value = texture);

            return texture;
        }

        public AsyncTexture2D GetRefTexture(string file)
        {
            var texture = RefTextures.FirstOrDefault(t => t.Key.Equals(file)).Value;

            if (texture != null)
                return texture;

            texture = contentsManager.GetTexture(file);

            if (texture != null)
                RefTextures.AddOrUpdate(file, texture, (key, value) => value = texture);

            return texture;
        }
    }
}
