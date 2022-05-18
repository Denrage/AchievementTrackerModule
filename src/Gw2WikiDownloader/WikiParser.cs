// See https://aka.ms/new-console-template for more information
using Denrage.AchievementTrackerModule.Libs.Achievement;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;

namespace Gw2WikiDownload;

public class WikiParser
{
    // Parses a overview category page. Used for the initial list of achievement categories. https://wiki.guildwars2.com/index.php?title=Category:Achievement_categories
    public IEnumerable<(string link, string title)> ParseListElementsFromWiki(string categoryOverviewSource)
    {
        var collections = categoryOverviewSource.Split('\n').Where(x => x.StartsWith("<li>") || x.StartsWith("<ul><li>"));

        var result = new List<AchievementPage>();

        foreach (var collection in collections)
        {
            var linkIdentifierIndex = collection.IndexOf("href");
            var linkFirstIndex = collection.IndexOf("\"", linkIdentifierIndex);
            var linkLastIndex = collection.IndexOf("\"", linkFirstIndex + 1);
            var titleIdentifierIndex = collection.IndexOf("title");
            var titleFirstIndex = collection.IndexOf("\"", titleIdentifierIndex);
            var titleLastIndex = collection.IndexOf("\"", titleFirstIndex + 1);
            var link = collection.Substring(linkFirstIndex + 1, linkLastIndex - linkFirstIndex - 1);
            var title = WebUtility.HtmlDecode(collection.Substring(titleFirstIndex + 1, titleLastIndex - titleFirstIndex - 1));

            yield return (link, title);
        }
    }

    public IEnumerable<AchievementTableEntry> Parse(HtmlDocument fullDocument, HtmlNode table) // Parse Overviewpage f.e. https://wiki.guildwars2.com/wiki/A_Bug_in_the_System_(achievements)
    {
        try
        {
            var groupedAchievements = new Dictionary<string, List<HtmlNode>>();
            var achievementNodes = table.SelectNodes("//tr[@data-id]");
            foreach (var achievementNode in achievementNodes)
            {
                var achievementId = achievementNode.Attributes["data-id"].Value;

                if (!groupedAchievements.ContainsKey(achievementId))
                {
                    groupedAchievements[achievementId] = new List<HtmlNode>();
                }

                groupedAchievements[achievementId].Add(achievementNode);
            }

            var result = new List<AchievementTableEntry>();

            foreach ((var key, var achievement) in groupedAchievements)
            {
                var achievementName = this.ParseHeaderRow(achievement[0].ChildNodes.FindFirst("th"));
                var entry = new AchievementTableEntry()
                {
                    Id = int.Parse(key.Replace("achievement", string.Empty)),
                    Name = achievementName.Name,
                    Link = achievementName.Link,
                };

                this.ParseDescriptionRow(fullDocument.DocumentNode, achievement[1], entry);
                result.Add(entry);
            }

            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void ParseDescriptionRow(HtmlNode fullDocument, HtmlNode descriptionRow, AchievementTableEntry entry)
    {
        try
        {
            var gameText = string.Empty;
            var gameHint = string.Empty;
            var tableData = descriptionRow.ChildNodes.Where(x => x.Name == "td").First();

            foreach (var item in tableData.ChildNodes)
            {
                if (item.Name == "dl" || item.Name == "p" || item.Name == "span")
                {
                    break;
                }

                if (item.NodeType == HtmlNodeType.Text)
                {
                    gameText += item.InnerText;
                }
                else
                {
                    gameText += item.OuterHtml;
                }
            }

            if (entry.HasLink) // Parse Collection achievements on their details page
            {
                var innerHtml = new HtmlWeb();
                var innerDocument = innerHtml.Load("https://wiki.guildwars2.com" + entry.Link);
                var tableNode = innerDocument.DocumentNode.SelectNodes("//table[contains(@class, 'mech1 achievementbox table')]");
                if (tableNode != null && innerDocument.DocumentNode.InnerHtml.Contains("Collection:"))
                {
                    var tableBody = tableNode.FindFirst("tbody");
                    if (tableBody != null)
                    {
                        var tableRows = tableBody.ChildNodes.Where(x => x.Name == "tr").ToArray();
                        if (tableRows.Length > 1)
                        {
                            var descriptionListElement = tableRows[1].ChildNodes.FindFirst("dl");
                            if (descriptionListElement != null)
                            {
                                var ddElements = descriptionListElement.ChildNodes.Where(x => x.Name == "dd");
                                if (ddElements.Any())
                                {
                                    this.ParseDescriptionList(entry, ddElements);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var node in tableData.ChildNodes)
            {
                if (node.Name == "dl" && entry.Description is null) // Multiarea description (Collection, Titles, Rewards, Objectives)
                {
                    this.ParseDescriptionList(entry, node.ChildNodes.Where(x => x.Name == "dd"));
                }
                else if (node.Name == "p") // Additional notes with cites
                {
                    var referenceNodes = fullDocument.SelectNodes("//ol[contains(@class, 'references')]");
                    if (referenceNodes.Any())
                    {
                        var relevantNode = referenceNodes.First();
                        var references = relevantNode.ChildNodes.Where(x => x.Name == "li");

                        var citeLink = node.ChildNodes.FindFirst("sup").ChildNodes.FindFirst("a").GetAttributeValue("href", string.Empty)[1..];
                        entry.Cite = references.First(x => x.GetAttributeValue("id", string.Empty) == citeLink).InnerText;

                    }
                }
                else if (node.Name == "span") // Game hint text
                {
                    gameHint = node.InnerText;
                }
            }

            if (entry.Description is null)
            {
                entry.Description = new StringDescription();
            }

            entry.Description.GameText = gameText;
            entry.Description.GameHint = gameHint;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public (string Name, string Link) ParseHeaderRow(HtmlNode headerRow)
    {
        var name = string.Empty;
        var link = string.Empty;

        foreach (var node in headerRow.ChildNodes)
        {
            if (node.NodeType == HtmlNodeType.Text && !string.IsNullOrEmpty(node.InnerText)) // Text only header
            {
                name = node.InnerText;
            }
            else if (node.Name == "a")
            {
                if (node.ChildNodes.Any(x => x.Name == "img")) // Small icon in front
                {

                }
                else // Link to a wiki page for this achievement
                {
                    link = node.GetAttributeValue("href", string.Empty);
                    name = SanitizesDisplayName(node.InnerText);

                    return (name, link);
                }
            }
        }

        return (name, link);
    }

    private (string RewardImage, string RewardName, string RewardLink) ParseItemReward(HtmlNode ddElement)
    {
        try
        {
            var rewardImage = ddElement.ChildNodes.FindFirst("span").ChildNodes.FindFirst("a").ChildNodes.FindFirst("img").GetAttributeValue("src", string.Empty);
            var rewardNameAndLinkElement = ddElement.ChildNodes.FindFirst("a");
            var rewardName = rewardNameAndLinkElement.GetAttributeValue("title", string.Empty);
            var rewardLink = rewardNameAndLinkElement.GetAttributeValue("href", string.Empty);

            return (rewardImage, rewardName, rewardLink);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void ParseDescriptionList(AchievementTableEntry achievementTableEntry, IEnumerable<HtmlNode> ddElements)
    {
        try
        {
            foreach (var ddElement in ddElements)
            {
                if (ddElement.InnerHtml.Contains("Reward:")) // Item Reward
                {
                    var (rewardImage, rewardName, rewardLink) = this.ParseItemReward(ddElement);
                    achievementTableEntry.Reward = new ItemReward()
                    {
                        ImageUrl = rewardImage,
                        DisplayName = rewardName,
                        ItemUrl = rewardLink,
                    };
                }
                else if (ddElement.InnerHtml.Contains("Rewards:")) // Multitier Reward - cant exists with Item Reward
                {
                    var rewardRows = ddElement.ChildNodes.FindFirst("table").ChildNodes.FindFirst("tbody").ChildNodes.Where(x => x.Name == "tr");
                    var reward = new MultiTierReward();

                    foreach (var row in rewardRows)
                    {
                        if (row.InnerHtml.Contains("Rewards:") || row.InnerHtml.Contains("<b>...</b>"))
                        {
                            continue;
                        }

                        var tableData = row.ChildNodes.Where(x => x.Name == "td").ToArray();
                        var tier = int.Parse(tableData[1].InnerText.Replace(":", string.Empty).Trim());
                        var (rewardImage, rewardName, rewardLink) = this.ParseItemReward(tableData[2]);
                        reward.Tiers.Add(new MultiTierReward.TierReward()
                        {
                            DisplayName = rewardName,
                            ImageUrl = rewardImage,
                            ItemUrl = rewardLink,
                            Tier = tier,
                        });
                    }

                    achievementTableEntry.Reward = reward;
                }
                else if (ddElement.InnerHtml.Contains("Title:")) // Title - Can exist with Reward(s)
                {
                    achievementTableEntry.Title = new AchievementTitle()
                    {
                        Title = SanitizesDisplayName(ddElement.LastChild.InnerText)
                    };
                }
                else if (ddElement.InnerHtml.Contains("Objectives:")) // Objective list
                {
                    var description = new ObjectivesDescription();
                    var tableRows = ddElement.ChildNodes.FindFirst("table").ChildNodes.FindFirst("tbody").ChildNodes.Where(x => x.Name == "tr");
                    foreach (var row in tableRows)
                    {
                        if (row.InnerHtml.Contains("Objectives:") || row.InnerHtml.Contains("<b>...</b>"))
                        {
                            continue;
                        }

                        var entry = row.ChildNodes.FindFirst("td").ChildNodes.FindFirst("ul").ChildNodes.FindFirst("li");
                        var entryLink = entry.ChildNodes.FindFirst("a");
                        var displayName = entry.InnerText;
                        var link = string.Empty;

                        if (entryLink != null)
                        {
                            link = entryLink.GetAttributeValue("href", string.Empty);
                        }

                        description.EntryList.Add(new TableDescriptionEntry() { DisplayName = SanitizesDisplayName(displayName), Link = link });
                    }

                    achievementTableEntry.Description = description;
                }
                else if (ddElement.InnerHtml.Contains("Collection:")) // Collection Achievement. Problem: Collection Items are in the next dd-Element
                {
                    var description = new CollectionDescription();

                    var descriptionCollectionElement = ddElements.First(x => x.InnerHtml.Contains("-bit"));

                    foreach (var item in descriptionCollectionElement.ChildNodes.FindFirst("div").ChildNodes.Where(x => x.Name == "div"))
                    {
                        description.EntryList.Add(new CollectionDescriptionEntry() { DisplayName = SanitizesDisplayName(item.ChildNodes.FindFirst("a").GetAttributeValue("title", string.Empty)), Link = item.ChildNodes.FindFirst("a").GetAttributeValue("href", string.Empty), ImageUrl = item.ChildNodes.FindFirst("a").ChildNodes.FindFirst("img").GetAttributeValue("src", string.Empty) });
                    }

                    achievementTableEntry.Description = description;
                }
                else if (ddElement.InnerHtml.Contains("Prerequisite:")) // Prerequisite
                {
                    achievementTableEntry.Prerequisite = ddElement.LastChild.InnerText;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static string SanitizesDisplayName(string displayName)
        => string.Join(" ", WebUtility.HtmlDecode(displayName).Replace(Environment.NewLine, "").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

    //href="https://api.guildwars2.com/v2/items?ids=86993&lang=en"
    public int ParseItemWikiPage(HtmlNode itemBoxNode) // Parse ItemPage f.e. https://wiki.guildwars2.com/wiki/Reinforced_Olmakhan_Bandolier
    {
        var dlNode = itemBoxNode.ChildNodes.Where(x => x.ChildNodes.FirstOrDefault(x => x.Name == "dl") != null).FirstOrDefault();
        if (dlNode != null)
        {
            var relevantDlNode = dlNode.ChildNodes.FirstOrDefault(x => x.InnerHtml.Contains("API"));
            if (relevantDlNode is null)
            {
                return -1;
            }

            try
            {
                var apiNode = relevantDlNode.ChildNodes.Where(x => x.Name == "dd" && x.InnerHtml.Contains("API")).First().ChildNodes.FindFirst("a");
                var apiLink = apiNode.GetAttributeValue("href", string.Empty);
                var idStartIndex = apiLink.IndexOf("ids=") + "ids=".Length;
                var idEndIndex = apiLink.IndexOf("&");
                var idList = apiLink[idStartIndex..idEndIndex];

                return idList.Contains(",") ? int.Parse(idList.Split(",", StringSplitOptions.RemoveEmptyEntries)[0]) : int.Parse(idList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        return -1;
    }

    public CollectionAchievementTable? ParseCollectionAchievementPage(HtmlNode collectionAchievementPageNode) // Parse Collection Achievement page f.e. https://wiki.guildwars2.com/wiki/Lasting_Bonds:_What_Comes_Next
    {
        var result = new CollectionAchievementTable();

        var relevantTableNode = collectionAchievementPageNode.SelectNodes("//table[contains(@class, 'sortable table')]")?.First();

        if (relevantTableNode == null)
        {
            return null;
        }

        var relevantRows = relevantTableNode.ChildNodes.FindFirst("tbody").ChildNodes.Where(x => x.Name == "tr").ToList();
        var tableHead = relevantTableNode.ChildNodes.FindFirst("thead");
        if (tableHead != null)
        {
            relevantRows.InsertRange(0, tableHead.ChildNodes.Where(x => x.Name == "tr"));
        }

        var headerRows = relevantRows.Where(x => x.ChildNodes.Any(x => x.Name == "th") && !x.GetClasses().Contains("sortbottom") && !x.ChildNodes.Any(x => x.Name == "td")).ToArray();
        var rows = relevantRows.Where(x => x.ChildNodes.Any(x => x.Name == "td") && x.GetAttributeValue("style", string.Empty) != "display:none").ToArray();

        result.Entries = new List<List<CollectionAchievementTable.CollectionAchievementTableEntry>>();
        for (var i = 0; i < rows.Length; i++)
        {
            result.Entries.Add(new List<CollectionAchievementTable.CollectionAchievementTableEntry>());
        }

        if (headerRows.Length > 1)
        {
            var rowSpanHeader = headerRows[0].ChildNodes.Where(x => !string.IsNullOrEmpty(x.GetAttributeValue("rowspan", string.Empty))).First();
            var headerColumns = headerRows[1].ChildNodes.Where(x => x.Name == "th").ToArray();
            result.ColumnNames = new string[headerColumns.Length + 1];
            result.ColumnNames[0] = SanitizesDisplayName(rowSpanHeader.InnerText);
            for (var i = 0; i < headerColumns.Length; i++)
            {
                result.ColumnNames[i + 1] = SanitizesDisplayName(headerColumns[i].InnerText);
            }
        }
        else
        {
            var tableHeaderCells = headerRows[0].ChildNodes.Where(x => x.Name == "th").ToArray();
            result.ColumnNames = new string[tableHeaderCells.Length];
            for (var i = 0; i < tableHeaderCells.Length; i++)
            {
                var text = SanitizesDisplayName(tableHeaderCells[i].InnerText);
                if (string.IsNullOrEmpty(text))
                {
                    text = "Number";
                }

                result.ColumnNames[i] = text;
            }
        }

        for (var i = 0; i < rows.Length; i++)
        {
            var tableData = rows[i].ChildNodes.Where((x) => x.Name == "td" || x.Name == "th").ToArray();

            for (var j = 0; j < tableData.Length; j++)
            {
                var entry = this.ParseCollectionAchievementTableEntry(tableData[j]);

                if (entry is null)
                {
                    entry = new CollectionAchievementTable.CollectionAchievementTableEmptyEntry();
                }

                result.Entries[i].Add(entry);
            }
        }

        return result;
    }

    private CollectionAchievementTable.CollectionAchievementTableEntry? ParseCollectionAchievementTableEntry(HtmlNode entry)
    {
        CollectionAchievementTable.CollectionAchievementTableEntry? currentEntry = null;
        foreach (var item in entry.ChildNodes)
        {
            if (item.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(item.InnerText.Replace(Environment.NewLine, string.Empty).Trim()) && item.InnerText != "&#8201;" && item.InnerText != "&#160;")
            {
                return int.TryParse(SanitizesDisplayName(item.InnerText), out var number)
                    ? new CollectionAchievementTable.CollectionAchievementTableNumberEntry()
                    {
                        Number = number,
                    }
                    : new CollectionAchievementTable.CollectionAchievementTableStringEntry()
                    {
                        Text = entry.InnerHtml,
                    };
            }
            else if (item.Name == "span")
            {
                var spanClasses = item.GetClasses();
                if (spanClasses.Contains("item-icon") && entry.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Text).Select(x => SanitizesDisplayName(x.InnerText)).All(x => string.IsNullOrEmpty(x) || x == "&#8201;" || x == "&#160;")) // Item
                {
                    if (currentEntry is null)
                    {
                        currentEntry = new CollectionAchievementTable.CollectionAchievementTableItemEntry();
                    }

                    var itemEntry = (CollectionAchievementTable.CollectionAchievementTableItemEntry)currentEntry;

                    itemEntry.ImageUrl = item.ChildNodes.FindFirst("a").ChildNodes.FindFirst("img").GetAttributeValue("src", string.Empty);
                }
                else if (spanClasses.Contains("gw2-tpprice"))
                {
                    currentEntry = new CollectionAchievementTable.CollectionAchievementTableCoinEntry()
                    {
                        ItemId = int.Parse(item.GetAttributeValue("data-id", "0")),
                        Type = item.GetAttributeValue("data-info", "buy") == "buy" ? CollectionAchievementTable.CollectionAchievementTableCoinEntry.TradingPostType.Buy : CollectionAchievementTable.CollectionAchievementTableCoinEntry.TradingPostType.Sell,
                    };
                }
            }
            else if (item.Name == "a")
            {
                if (currentEntry is CollectionAchievementTable.CollectionAchievementTableItemEntry itemEntry)
                {
                    itemEntry.Link = item.GetAttributeValue("href", string.Empty);
                    itemEntry.Name = SanitizesDisplayName(item.GetAttributeValue("title", string.Empty));
                    Debug.WriteLine(itemEntry.Name);
                    var web = new HtmlWeb();
                    var document = web.LoadFromWebAsync("https://wiki.guildwars2.com" + itemEntry.Link).Result;
                    var itemBoxNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'infobox')]").FindFirst("div");
                    itemEntry.Id = this.ParseItemWikiPage(itemBoxNode);
                }
                else
                {
                    var imageNode = item.ChildNodes.FindFirst("img");

                    currentEntry = imageNode != null
                        ? new CollectionAchievementTable.CollectionAchievementTableMapEntry()
                        {
                            ImageLink = item.GetAttributeValue("href", string.Empty),
                        }
                        : new CollectionAchievementTable.CollectionAchievementTableLinkEntry
                        {
                            Link = item.GetAttributeValue("href", string.Empty),
                            Text = SanitizesDisplayName(item.GetAttributeValue("title", string.Empty))
                        };
                }
            }
            else if (item.Name == "i")
            {
                var stringEntry = new CollectionAchievementTable.CollectionAchievementTableStringEntry
                {
                    Text = item.InnerText
                };

                currentEntry = stringEntry;
            }
        }

        return currentEntry;
    }

    public async Task ParseSubPage(string link, int depth, List<SubPageInformation> results, int maxDepth = 3)
    {
        try
        {
            if (link.Count(x => x == '.') > 2) // Skip anything that is not from the gw2 wiki
            {
                return;
            }

            if (link.IndexOf("#") > 0)
            {
                link = link[..link.IndexOf("#")];
            }

            Console.WriteLine($"{link} - {depth}");

            if (depth > maxDepth)
            {
                return;
            }

            var web = new HtmlWeb();
            var document = await web.LoadFromWebAsync(link);
            SubPageInformation subPageInformation = null;

            if (document.DocumentNode.OuterHtml.Contains("infobox npc")) // NPC subpage https://wiki.guildwars2.com/wiki/Efi
            {
                subPageInformation = this.ParseNpcSubPage(document);
            }
            else if (document.DocumentNode.OuterHtml.Contains("infobox area")) // Some map type (POI, etc.) https://wiki.guildwars2.com/wiki/Magnetics_Lab
            {
                subPageInformation = this.ParseLocationSubPage(document);
            }
            else if (document.DocumentNode.OuterHtml.Contains("infobox quest")) // Event https://wiki.guildwars2.com/wiki/Defeat_Abiri,_the_Beetle_Queen
            {
                subPageInformation = this.ParseQuestSubPage(document);

            }
            else if (document.DocumentNode.OuterHtml.Contains("infobox item")) // Item https://wiki.guildwars2.com/wiki/Bundle_of_Children's_Toys
            {
                subPageInformation = this.ParseItemSubPage(document);
            }
            else // If nothing fits, just show a summary https://wiki.guildwars2.com/wiki/Bounty
            {
                subPageInformation = new TextSubPageInformation();
            }

            subPageInformation.Title = SanitizesDisplayName(document.DocumentNode.SelectSingleNode("//h1[@id='firstHeading']").InnerText);
            subPageInformation.Link = link;

            var pageNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'mw-parser-output')]");
            var firstText = new List<HtmlNode>();

            foreach (var item in pageNode.ChildNodes)
            {
                if (item.Name == "p")
                {
                    firstText.Add(item);
                }

                if (firstText.Any() && item.Name != "p")
                {
                    break;
                }
            }
            var descriptionNode = HtmlNode.CreateNode("<div>" + string.Join("", firstText.Select(x => x.InnerHtml)) + "</div>");
            subPageInformation.Description = descriptionNode.InnerHtml;
            results.Add(subPageInformation);
            foreach (var linkNode in descriptionNode.ChildNodes.Where(x => x.Name == "a"))
            {
                if (linkNode.GetAttributeValue("class", "").Contains("selflink"))
                {
                    continue;
                }

                var subPageLink = linkNode.GetAttributeValue("href", "");

                if (!subPageLink.Contains('.')) // Skip anything that is not on the wiki.guildwars2.com domain
                {
                    subPageLink = "https://wiki.guildwars2.com" + subPageLink;

                    if (subPageLink.IndexOf("#") > 0)
                    {
                        subPageLink = subPageLink[..subPageLink.IndexOf("#")];
                    }

                    if (results.Select(x => x.Link).Contains(subPageLink))
                    {
                        continue;
                    }

                    await this.ParseSubPage(subPageLink, depth + 1, results, maxDepth);
                }
            }

            if (subPageInformation is IHasDescriptionList descriptionListPage)
            {
                foreach (var item in descriptionListPage.DescriptionList)
                {
                    var keyNode = HtmlNode.CreateNode("<div>" + item.Key + "</div>");
                    var valueNode = HtmlNode.CreateNode("<div>" + item.Value + "</div>");
                    foreach (var node in keyNode.ChildNodes.Where(x => x.Name == "a").Concat(valueNode.ChildNodes.Where(x => x.Name == "a")))
                    {
                        if (node.GetAttributeValue("class", "").Contains("selflink"))
                        {
                            continue;
                        }

                        var subPageLink = node.GetAttributeValue("href", "");

                        if (!subPageLink.Contains('.')) // Skip anything that is not on the wiki.guildwars2.com domain
                        {
                            subPageLink = "https://wiki.guildwars2.com" + subPageLink;

                            if (subPageLink.IndexOf("#") > 0)
                            {
                                subPageLink = subPageLink[..subPageLink.IndexOf("#")];
                            }

                            if (results.Select(x => x.Link).Contains(subPageLink))
                            {
                                continue;
                            }

                            await this.ParseSubPage(subPageLink, depth + 1, results, maxDepth);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private NpcSubPageInformation ParseNpcSubPage(HtmlDocument document)
    {
        var npcSubPage = new NpcSubPageInformation();
        var npcInfoBox = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'infobox npc')]");
        HtmlNode element = null;
        var wrapper = npcInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "div");

        if (wrapper != null)
        {
            element = wrapper;
        }
        else
        {
            if (npcInfoBox.ChildNodes.Any(x => x.Name == "dl"))
            {
                element = npcInfoBox.ChildNodes.First(x => x.Name == "dl");
            }
        }

        if (element != null)
        {
            npcSubPage.DescriptionList = this.ParseDescriptionList(element);
        }

        npcSubPage.ImageUrl = this.ParseInfoBoxImageWrapper(npcInfoBox);

        var table = npcInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "table");

        var imagePart = this.ParseAdditionalImagesPartInfoBox(table);
        npcSubPage.AdditionalImages.AddRange(imagePart.Images);
        npcSubPage.InteractiveMap = imagePart.MapInformation;

        return npcSubPage;
    }

    private LocationSubPageInformation ParseLocationSubPage(HtmlDocument document)
    {
        var subPage = new LocationSubPageInformation();
        var locationInfoBox = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'infobox area')]");
        HtmlNode element = null;
        var wrapper = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "div");

        if (wrapper != null)
        {
            element = wrapper;
        }
        else
        {
            if (locationInfoBox.ChildNodes.Any(x => x.Name == "dl"))
            {
                element = locationInfoBox.ChildNodes.First(x => x.Name == "dl");
            }
        }

        if (element != null)
        {
            subPage.DescriptionList = this.ParseDescriptionList(element);
        }

        subPage.ImageUrl = this.ParseInfoBoxImageWrapper(locationInfoBox);
        var table = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "table");

        var imagePart = this.ParseAdditionalImagesPartInfoBox(table);
        subPage.AdditionalImages.AddRange(imagePart.Images);
        subPage.InteractiveMap = imagePart.MapInformation;

        return subPage;
    }

    private QuestSubPageInformation ParseQuestSubPage(HtmlDocument document)
    {
        var subPage = new QuestSubPageInformation();
        var locationInfoBox = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'infobox quest')]");
        HtmlNode element = null;
        var wrapper = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "div");

        if (wrapper != null)
        {
            element = wrapper;
        }
        else
        {
            if (locationInfoBox.ChildNodes.Any(x => x.Name == "dl"))
            {
                element = locationInfoBox.ChildNodes.First(x => x.Name == "dl");
            }
        }

        if (element != null)
        {
            subPage.DescriptionList = this.ParseDescriptionList(element);
        }

        subPage.ImageUrl = this.ParseInfoBoxImageWrapper(locationInfoBox);

        var table = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "table");

        var imagePart = this.ParseAdditionalImagesPartInfoBox(table);
        subPage.AdditionalImages.AddRange(imagePart.Images);
        subPage.InteractiveMap = imagePart.MapInformation;

        return subPage;
    }

    private ItemSubPageInformation ParseItemSubPage(HtmlDocument document)
    {
        var subPage = new ItemSubPageInformation();
        var locationInfoBox = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'infobox item')]");
        HtmlNode element = null;
        var wrapper = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "div" && x.GetClasses().Contains("wrapper"));

        if (wrapper != null)
        {
            element = wrapper;
        }
        else
        {
            if (locationInfoBox.ChildNodes.Any(x => x.Name == "dl"))
            {
                element = locationInfoBox.ChildNodes.First(x => x.Name == "dl");
            }
        }

        if (element != null)
        {
            subPage.DescriptionList = this.ParseDescriptionList(element);
        }

        var iconElement = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "div" && x.GetClasses().Contains("infobox-icon"));

        if (iconElement != null)
        {
            subPage.ImageUrl = iconElement.ChildNodes.First(x => x.Name == "a").GetAttributeValue("href", "");
        }

        var table = locationInfoBox.ChildNodes.FirstOrDefault(x => x.Name == "table");

        var imagePart = this.ParseAdditionalImagesPartInfoBox(table);
        subPage.AdditionalImages.AddRange(imagePart.Images);
        subPage.InteractiveMap = imagePart.MapInformation;

        return subPage;
    }

    private string ParseInfoBoxImageWrapper(HtmlNode infoBox)
    {
        var wrapper = infoBox.SelectSingleNode("//p[contains(@class, 'image_wrapper')]");

        if (wrapper != null)
        {
            if (wrapper.ChildNodes.Any())
            {
                var linkElement = wrapper.ChildNodes.FirstOrDefault(x => x.Name == "a");
                if (linkElement != null)
                {
                    return linkElement.GetAttributeValue("href", "");
                }
            }
        }

        return string.Empty;
    }

    private List<KeyValuePair<string, string>> ParseDescriptionList(HtmlNode wrapper)
    {
        var descriptionListResult = new List<KeyValuePair<string, string>>();

        var descriptionLists = wrapper.ChildNodes.Where(x => x.Name == "dl");

        if (descriptionLists.Any())
        {
            var descriptionListEntries = descriptionLists.SelectMany(x => x.ChildNodes).Where(x => x.Name == "dt" || x.Name == "dd").ToArray();
            for (int i = 0; i < descriptionListEntries.Length; i += 2)
            {
                // The dd Element "always" (till it randomly is not doing it) follows the dt Element
                if (i + 1 != descriptionListEntries.Length)
                {
                    descriptionListResult.Add(new KeyValuePair<string, string>(descriptionListEntries[i].InnerHtml, descriptionListEntries[i + 1].InnerHtml));
                }
                else // https://wiki.guildwars2.com/wiki/Help_Elof_recover_buried_artifacts
                {
                    var weirdOutsideDescriptionElement = wrapper.ChildNodes.First(x => x.Name == "dd");
                    var relevantElements = weirdOutsideDescriptionElement.ChildNodes.Where(x => x.Name == "a").ToArray();
                    var insideDiv = weirdOutsideDescriptionElement.ChildNodes.First(x => x.Name == "div");
                    relevantElements = relevantElements.Concat(insideDiv.ChildNodes.Where(x => x.Name == "a")).ToArray();
                    descriptionListResult.Add(new KeyValuePair<string, string>(descriptionListEntries[i].InnerHtml, string.Join(", ", relevantElements.Select(x => x.OuterHtml))));
                }
            }
        }

        return descriptionListResult;
    }

    private (IEnumerable<string> Images, InteractiveMapInformation MapInformation) ParseAdditionalImagesPartInfoBox(HtmlNode table)
    {
        var result = new List<string>();
        InteractiveMapInformation map = null;
        if (table != null)
        {
            var tableBody = table.ChildNodes.FindFirst("tbody");
            var relevantRows = tableBody.ChildNodes.Where(x => x.Name == "tr").Skip(1);

            foreach (var item in relevantRows)
            {
                var data = item.ChildNodes.FindFirst("td");

                // TODO: Parse interactive map through string manipulation and reverse engineering of the script tag
                var linkNodes = data.ChildNodes.Where(x => x.Name == "a");
                if (linkNodes.Any())
                {
                    foreach (var linkNode in linkNodes)
                    {
                        result.Add(linkNode.GetAttributeValue("href", ""));
                    }
                }

                var pElement = data.ChildNodes.FindFirst("p");
                if (pElement != null)
                {
                    var linkElement = pElement.ChildNodes.FindFirst("a");

                    if (linkElement != null)
                    {
                        result.Add(linkElement.GetAttributeValue("href", ""));
                    }
                }


                var scriptNodes = data.ChildNodes.Where(x => x.Name == "script");
                if (scriptNodes.Any())
                {
                    foreach (var script in scriptNodes)
                    {
                        if (!string.IsNullOrEmpty(script.InnerText))
                        {
                            map = this.ParseInteractiveMap(script.InnerText);
                        }
                    }
                }
            }
        }

        return (result, map);
    }

    private InteractiveMapInformation ParseInteractiveMap(string scriptTag)
    {
        var inputInformationStartIndex = scriptTag.IndexOf("// Inputs");
        var inputInformationEndIndex = scriptTag.IndexOf("// If all inputs", inputInformationStartIndex);
        var relevantString = scriptTag[inputInformationStartIndex..inputInformationEndIndex];

        var iconUrlStartIndex = relevantString.IndexOf("\"") + 1;
        var iconUrlEndIndex = relevantString.IndexOf("\"", iconUrlStartIndex);
        var parsedIconUrl = relevantString[iconUrlStartIndex..iconUrlEndIndex];
        var iconUrl = string.IsNullOrEmpty(parsedIconUrl) ? string.Empty : "https:" + parsedIconUrl;

        var localTilesStartIndex = relevantString.IndexOf("\"", iconUrlEndIndex + 1) + 1;
        var localTilesEndIndex = relevantString.IndexOf("\"", localTilesStartIndex);
        var localTiles = relevantString[localTilesStartIndex..localTilesEndIndex];

        var coordsStartIndex = relevantString.IndexOf("\"", localTilesEndIndex + 1) + 1;
        var coordsEndIndex = relevantString.IndexOf("\"", coordsStartIndex);
        var coords = relevantString[coordsStartIndex..coordsEndIndex];

        var pathStartIndex = relevantString.IndexOf("\"", coordsEndIndex + 1) + 1;
        var pathEndIndex = relevantString.IndexOf("\"", pathStartIndex);
        var path = relevantString[pathStartIndex..pathEndIndex];

        var boundsStartIndex = relevantString.IndexOf("\"", pathEndIndex + 1) + 1;
        var boundsEndIndex = relevantString.IndexOf("\"", boundsStartIndex);
        var bounds = relevantString[boundsStartIndex..boundsEndIndex];

        return new InteractiveMapInformation()
        {
            Bounds = bounds,
            IconUrl = iconUrl,
            LocalTiles = localTiles,
            Path = path,
            Coordinates = coords,
        };
    }

    public interface IHasDescriptionList
    {
        List<KeyValuePair<string, string>> DescriptionList { get; set; }
    }

    public interface IHasInteractiveMap
    {
        InteractiveMapInformation InteractiveMap { get; set; }
    }

    public abstract class SubPageInformation
    {
        public string Title { get; set; }

        public string Link { get; set; }

        public string Description { get; set; }
    }

    public class NpcSubPageInformation : SubPageInformation, IHasDescriptionList, IHasInteractiveMap
    {
        public string ImageUrl { get; set; }

        public List<KeyValuePair<string, string>> DescriptionList { get; set; } = new List<KeyValuePair<string, string>>();

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public InteractiveMapInformation InteractiveMap { get; set; }
    }

    public class LocationSubPageInformation : SubPageInformation, IHasDescriptionList, IHasInteractiveMap
    {
        public string ImageUrl { get; set; }

        public List<KeyValuePair<string, string>> DescriptionList { get; set; } = new List<KeyValuePair<string, string>>();

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public InteractiveMapInformation InteractiveMap { get; set; }
    }

    public class QuestSubPageInformation : SubPageInformation, IHasDescriptionList, IHasInteractiveMap
    {
        public string ImageUrl { get; set; }

        public List<KeyValuePair<string, string>> DescriptionList { get; set; } = new List<KeyValuePair<string, string>>();

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public InteractiveMapInformation InteractiveMap { get; set; }
    }

    public class ItemSubPageInformation : SubPageInformation, IHasDescriptionList, IHasInteractiveMap
    {
        public string ImageUrl { get; set; }

        public List<KeyValuePair<string, string>> DescriptionList { get; set; } = new List<KeyValuePair<string, string>>();

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public InteractiveMapInformation InteractiveMap { get; set; }
    }

    public class TextSubPageInformation : SubPageInformation { }

    public class InteractiveMapInformation
    {
        public string IconUrl { get; set; }

        public string LocalTiles { get; set; }

        public string Path { get; set; }

        public string Bounds { get; set; }

        public string Coordinates { get; set; }
    }
}
