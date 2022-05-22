using Blish_HUD;
using Blish_HUD.Content;
using Denrage.AchievementTrackerModule.Interfaces;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Services
{
    public class ExternalImageService : IExternalImageService
    {
        private readonly GraphicsService graphicsService;
        private readonly Logger logger;

        public ExternalImageService(GraphicsService graphicsService, Logger logger)
        {
            this.graphicsService = graphicsService;
            this.logger = logger;
        }

        public AsyncTexture2D GetImage(string imageUrl)
            => this.GetImageInternal((async () => await this.DownloadWikiContent(imageUrl).GetStreamAsync(), imageUrl));

        public async Task<string> GetDirectImageLink(string imagePath, CancellationToken cancellationToken = default)
        {
            if (imagePath.Contains("File:"))
            {
                try
                {
                    var source = await this.DownloadWikiContent(imagePath).GetStringAsync(cancellationToken);

                    var fillImageStartIndex = source.IndexOf("fullImageLink");
                    var hrefStartIndex = source.IndexOf("href=", fillImageStartIndex);
                    var linkStartIndex = source.IndexOf("\"", hrefStartIndex) + 1;
                    var linkEndIndex = source.IndexOf("\"", linkStartIndex);
                    var link = source.Substring(linkStartIndex, linkEndIndex - linkStartIndex);
                    return link;
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Exception occured on parsing an image path");
                    return string.Empty;
                }
            }

            return imagePath;
        }

        public AsyncTexture2D GetImageFromIndirectLink(string imagePath)
            => _ = this.GetImageInternal((async () => await this.DownloadWikiContent(await this.GetDirectImageLink(imagePath)).GetStreamAsync(), imagePath));

        private AsyncTexture2D GetImageInternal((Func<Task<Stream>> GetStream, string Url) getImageStream)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);

            _ = Task.Run(async () =>
            {
                try
                {
                    var imageStream = await getImageStream.GetStream();

                    this.graphicsService.QueueMainThreadRender(device =>
                    {
                        try
                        {
                            texture.SwapTexture(TextureUtil.FromStreamPremultiplied(device, imageStream));
                            imageStream.Close();
                        }
                        catch (Exception ex)
                        {
                            this.logger.Error(ex, $"Exception occured on downloading/swapping image. URL: {getImageStream.Url}");

                            this.graphicsService.QueueMainThreadRender(_ => texture.SwapTexture(ContentService.Textures.Error));
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, $"Exception occured on downloading/swapping image. URL: {getImageStream.Url}");

                    this.graphicsService.QueueMainThreadRender(_ => texture.SwapTexture(ContentService.Textures.Error));
                }
            });

            return texture;
        }

        private IFlurlRequest DownloadWikiContent(string url)
            => ("https://wiki.guildwars2.com" + url)
                    .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36");
    }
}
