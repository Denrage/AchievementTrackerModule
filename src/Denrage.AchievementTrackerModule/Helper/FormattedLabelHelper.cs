using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using Denrage.AchievementTrackerModule.UserInterface.Controls.FormattedLabel;
using Denrage.AchievementTrackerModule.UserInterface.Windows;
using Flurl.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.Helper
{
    internal class FormattedLabelHelper
    {
        private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
        public static IAchievementService AchievementService { get; set; }
        public static ContentsManager ContentsManager { get; set; }

        public static FormattedLabelBuilder CreateLabel(string textWithHtml)
        {
            var labelBuilder = new FormattedLabelBuilder();

            var node = HtmlNode.CreateNode("<div>" + textWithHtml + "</div>");

            foreach (var childNode in node.ChildNodes)
            {
                foreach (var item in CreateParts(childNode, labelBuilder))
                {
                    _ = labelBuilder.CreatePart(item);
                }
            }

            return labelBuilder;
        }

        private static IEnumerable<FormattedLabelPartBuilder> CreateParts(HtmlNode childNode, FormattedLabelBuilder labelBuilder)
        {
            if (childNode.Name == "#text")
            {
                yield return labelBuilder.CreatePart(childNode.InnerText);
            }
            else if (childNode.Name == "a")
            {
                // TODO: Check for more
                if (!childNode.GetClasses().Contains("mw-selflink"))
                {
                    foreach (var innerChildNode in childNode.ChildNodes)
                    {
                        foreach (var part in CreateParts(innerChildNode, labelBuilder))
                        {
                            var link = childNode.GetAttributeValue("href", "");
                            var inSubpages = false;
                            foreach (var subPage in AchievementService.Subpages)
                            {
                                if (subPage.Link.Contains(link) && !inSubpages )
                                {
                                    inSubpages = true;
                                    yield return part.SetLink(() =>
                                    {
                                        var window = new SubPageInformationWindow(ContentsManager, AchievementService, subPage)
                                        {
                                            Parent = GameService.Graphics.SpriteScreen,
                                        };

                                        window.Hidden += (s, e) => window.Dispose();
                                        window.Show();
                                    }).MakeUnderlined();
                                }
                            }

                            if (!inSubpages)
                            {
                                if (link.StartsWith("/"))
                                {
                                    link = "https://wiki.guildwars2.com/" + link;
                                }

                                yield return part.SetHyperLink(link).MakeUnderlined();
                            }
                        }
                    }
                }
                else
                {
                    foreach (var innerChildNode in childNode.ChildNodes)
                    {
                        foreach (var part in CreateParts(innerChildNode, labelBuilder))
                        {
                            yield return part;
                        }
                    }
                }
            }
            else if (childNode.Name == "span")
            {
                // TODO: Check for more
                if (childNode.GetClasses().Contains("inline-icon"))
                {
                    var imageNode = childNode.ChildNodes.FindFirst("img");

                    if (imageNode != null)
                    {
                        var builder = labelBuilder.CreatePart("");
                        builder.SetPrefixImage(GetTexture(imageNode.GetAttributeValue("src", ""))).SetPrefixImageSize(new Microsoft.Xna.Framework.Point(24, 24));
                        yield return builder;
                    }
                }
                else
                {
                    foreach (var innerChildNode in childNode.ChildNodes)
                    {
                        foreach (var part in CreateParts(innerChildNode, labelBuilder))
                        {
                            yield return part;
                        }
                    }
                }
            }
            else if (childNode.Name == "b")
            {
                // TODO: Make it bold when merged with core
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "i")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part.MakeItalic();
                    }
                }
            }
            else if (childNode.Name == "sup")
            {
                // TODO: Ignore for now
                yield return labelBuilder.CreatePart(string.Empty);
            }
            else if (childNode.Name == "h3")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "style" || childNode.Name == "script")
            {
                // TODO: Ignore for now
                yield return labelBuilder.CreatePart(string.Empty);
            }
            else if (childNode.Name == "p")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "br")
            {
                yield return labelBuilder.CreatePart("\n");
            }
            else if (childNode.Name == "small")
            {
                // TODO: Make it small when merged with core
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "ul")
            {
                // TODO: Does this work?
                foreach (var item in childNode.ChildNodes.Where(x => x.Name == "li"))
                {
                    foreach (var part in CreateParts(item, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "li")
            {
                // TODO: Does this work?
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "div")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "code")
            {
                yield return labelBuilder.CreatePart(childNode.InnerText);
            }
            else if (childNode.Name == "img")
            {
                var builder = labelBuilder.CreatePart(string.Empty);
                yield return builder.SetPrefixImage(GetTexture(childNode.GetAttributeValue("src", string.Empty)));
            }
            else if (childNode.Name == "s")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part.MakeStrikeThrough();
                    }
                }
            }
            else if (childNode.Name == "dl")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "font")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "big")
            {
                // TODO: Make it big when merged with core
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "ol")
            {
                // TODO: Does this work?
                foreach (var item in childNode.ChildNodes.Where(x => x.Name == "li"))
                {
                    foreach (var part in CreateParts(item, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
        }

        private static AsyncTexture2D GetTexture(string url)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);
            _ = Task.Run(() =>
            {
                try
                {
                    var imageStream = ("https://wiki.guildwars2.com" + url).WithHeader("user-agent", USER_AGENT).GetStreamAsync().Result;

                    GameService.Graphics.QueueMainThreadRender(device =>
                    {
                        texture.SwapTexture(TextureUtil.FromStreamPremultiplied(device, imageStream));
                        imageStream.Close();
                    });
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is FlurlHttpException httpException)
                    {
                        if (!httpException.Message.Contains("404 (Not Found)")) // Ignore 404 Errors
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            });

            return texture;
        }
    }
}
