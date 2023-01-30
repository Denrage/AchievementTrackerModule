using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Denrage.AchievementTrackerModule.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AchievementTrackerModule.Services
{
    public class FormattedLabelHtmlService : IFormattedLabelHtmlService
    {
        private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
        private readonly IAchievementService achievementService;
        private readonly ISubPageInformationWindowManager subPageInformationWindowManager;
        private readonly IExternalImageService externalImageService;
        public readonly ContentsManager contentsManager;

        public FormattedLabelHtmlService(ContentsManager contentsManager, IAchievementService achievementService, ISubPageInformationWindowManager subPageInformationWindowManager, IExternalImageService externalImageService)
        {
            this.contentsManager = contentsManager;
            this.achievementService = achievementService;
            this.subPageInformationWindowManager = subPageInformationWindowManager;
            this.externalImageService = externalImageService;
        }

        public FormattedLabelBuilder CreateLabel(string textWithHtml)
        {
            var labelBuilder = new FormattedLabelBuilder();

            var node = HtmlNode.CreateNode("<div>" + textWithHtml + "</div>");

            foreach (var childNode in node.ChildNodes)
            {
                foreach (var item in this.CreateParts(childNode, labelBuilder))
                {
                    _ = labelBuilder.CreatePart(item);
                }
            }

            return labelBuilder;
        }

        private IEnumerable<FormattedLabelPartBuilder> CreateParts(HtmlNode childNode, FormattedLabelBuilder labelBuilder)
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
                        foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
                        {
                            var link = childNode.GetAttributeValue("href", "");
                            var inSubpages = false;
                            foreach (var subPage in this.achievementService.Subpages)
                            {
                                if (subPage.Link == "https://wiki.guildwars2.com" + link && !inSubpages)
                                {
                                    inSubpages = true;
                                    yield return part.SetLink(() => this.subPageInformationWindowManager.Create(subPage)).MakeUnderlined();
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
                        foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                        _ = builder.SetPrefixImage(this.externalImageService.GetImage(imageNode.GetAttributeValue("src", ""))).SetPrefixImageSize(new Microsoft.Xna.Framework.Point(24, 24));
                        yield return builder;
                    }
                }
                else
                {
                    foreach (var innerChildNode in childNode.ChildNodes)
                    {
                        foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "i")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(item, labelBuilder))
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
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "div")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                yield return builder.SetPrefixImage(this.externalImageService.GetImage(childNode.GetAttributeValue("src", string.Empty)));
            }
            else if (childNode.Name == "s")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part.MakeStrikeThrough();
                    }
                }
            }
            else if (childNode.Name == "dl")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
            else if (childNode.Name == "font")
            {
                foreach (var innerChildNode in childNode.ChildNodes)
                {
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(innerChildNode, labelBuilder))
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
                    foreach (var part in this.CreateParts(item, labelBuilder))
                    {
                        yield return part;
                    }
                }
            }
        }
    }
}
