using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;

namespace Denrage.AchievementTrackerModule.Services
{
    public interface ITextureService
    {
        AsyncTexture2D GetTexture(string url);
    }
    public class TextureService : ITextureService
    {
        private ContentService contentService { get;  }

        private ConcurrentDictionary<string, AsyncTexture2D> Textures { get; set; }

        public TextureService(ContentService contentService)
        {
            Textures = new ConcurrentDictionary<string, AsyncTexture2D>();
            this.contentService = contentService;

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
    }
}
