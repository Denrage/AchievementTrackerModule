// See https://aka.ms/new-console-template for more information
using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;
var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36";

Console.WriteLine("Hello, World!");
var outputFolder = "WikiPages";

Directory.CreateDirectory(outputFolder);
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("user-agent", userAgent);
var request = new HttpRequestMessage(HttpMethod.Get, "https://wiki.guildwars2.com/index.php?title=Category:Achievement_categories");

var response = await httpClient.SendAsync(request);


var parser = new Gw2WikiDownload.WikiParser();

//var document = new HtmlAgilityPack.HtmlDocument();
//document.LoadHtml(File.ReadAllText("Untitled-1.html"));
//var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");

//var result = parser.Parse(document, tables.First());
var listElements = parser.ParseListElementsFromWiki(await response.Content.ReadAsStringAsync()).Skip(4);
var result = new List<Gw2WikiDownload.WikiParser.AchievementTableEntry>();
foreach (var item in listElements)
{
    if (item.title == "Living World Dailies")
    {
        System.Diagnostics.Debug.WriteLine(item.title + " skipped");
        continue;
    }
    System.Diagnostics.Debug.WriteLine(item.title);
    var web = new HtmlWeb();
    var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.link);
    var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
    result.AddRange(parser.Parse(document, tables.First()));
}




Console.WriteLine("Done");


public class AchievementPage
{
    public string Title { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;
}


namespace Gw2WikiDownload
{
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
                var title = WebUtility.HtmlDecode(collection.Substring(titleFirstIndex + 1, titleLastIndex - titleFirstIndex - 1)).Replace(":", "}");

                yield return (link, title);
            }
        }

        // TODO: cites (sup)
        public IEnumerable<AchievementTableEntry> Parse(HtmlDocument fullDocument, HtmlNode table)
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

                foreach (var achievement in groupedAchievements.Values)
                {
                    var achievementName = ParseHeaderRow(achievement[0].ChildNodes.FindFirst("th"));
                    var entry = new AchievementTableEntry(achievementName.Name, link: achievementName.Link);
                    ParseDescriptionRow(fullDocument.DocumentNode, achievement[1], entry);
                    result.Add(entry);
                }

                return result;
            }
            catch (Exception ex)
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

                foreach (var node in descriptionRow.ChildNodes.Where(x => x.Name == "td").First().ChildNodes)
                {
                    if (node.NodeType == HtmlNodeType.Text && !string.IsNullOrEmpty(node.InnerText.Replace(Environment.NewLine, string.Empty))) // Description text
                    {
                        gameText = node.InnerText;
                    }
                    else if (node.Name == "dl") // Multiarea description (Collection, Titles, Rewards, Objectives)
                    {
                        ParseDescriptionList(entry, node.ChildNodes.Where(x => x.Name == "dd"));
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
            catch (Exception ex)
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
            catch (Exception ex)
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
                        var (rewardImage, rewardName, rewardLink) = ParseItemReward(ddElement);
                        achievementTableEntry.Reward = new ItemReward(rewardImage, rewardName, rewardLink);
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
                            var (rewardImage, rewardName, rewardLink) = ParseItemReward(tableData[2]);
                            reward.Tiers.Add(new MultiTierReward.TierReward(rewardImage, rewardName, rewardLink, tier));
                        }

                        achievementTableEntry.Reward = reward;
                    }
                    else if (ddElement.InnerHtml.Contains("Title:")) // Title - Can exist with Reward(s)
                    {
                        achievementTableEntry.Title = new AchievementTitle(SanitizesDisplayName(ddElement.LastChild.InnerText));
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
                                displayName = entryLink.InnerText;
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
            catch (Exception ex )
            {

                throw;
            }
        }

        private static string SanitizesDisplayName(string displayName)
            => string.Join(" ", WebUtility.HtmlDecode(displayName).Replace(Environment.NewLine, "").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        [DebuggerDisplay("{Name}")]
        public class AchievementTableEntry
        {
            public string Name { get; set; } = string.Empty;

            public string? Link { get; set; }

            public bool HasLink => this.Link != null;

            public string? Prerequisite { get; set; }

            public AchievementTitle Title { get; set; }

            public Reward Reward { get; set; }

            public AchievementTableEntryDescription? Description { get; set; }

            public string? Cite { get; set; }

            public AchievementTableEntry(string name, AchievementTableEntryDescription? description = null, AchievementTitle? title = null, Reward? reward = null, string? prerequisite = null, string? link = null)
            {
                this.Name = name;
                this.Description = description;
                this.Reward = reward ?? Reward.EmptyReward;
                this.Link = link;
                this.Prerequisite = prerequisite;
                this.Title = title ?? AchievementTitle.EmptyTitle;
            }
        }

        public abstract class Reward
        {
            public static Reward EmptyReward { get; } = new EmptyReward();
        }

        [DebuggerDisplay("EmptyReward")]
        public class EmptyReward : Reward
        {

        }

        [DebuggerDisplay("{DisplayName}")]
        public class ItemReward : Reward
        {
            public string ImageUrl { get; set; }

            public string DisplayName { get; set; }

            public string ItemUrl { get; set; }

            public ItemReward(string imageUrl, string displayName, string itemUrl)
            {
                this.ImageUrl = imageUrl;
                this.DisplayName = displayName;
                this.ItemUrl = itemUrl;
            }
        }

        [DebuggerDisplay("{Tiers[0].DisplayName}")]
        public class MultiTierReward : Reward
        {
            public List<TierReward> Tiers { get; set; } = new List<TierReward>();

            public class TierReward : ItemReward
            {
                public int Tier { get; set; }

                public TierReward(string imageUrl, string displayName, string itemUrl, int tier)
                    : base(imageUrl, displayName, itemUrl)
                {
                    this.Tier = tier;
                }
            }
        }

        [DebuggerDisplay("{Title}")]
        public class AchievementTitle
        {
            public static AchievementTitle EmptyTitle { get; } = new AchievementTitle(string.Empty);

            public string Title { get; set; }

            public AchievementTitle(string title)
            {
                this.Title = title;
            }
        }

        public abstract class AchievementTableEntryDescription
        {
            public string? GameText { get; set; }

            public string? GameHint { get; set; }

            protected AchievementTableEntryDescription(string? gameText, string? gameHint)
            {
                this.GameText = gameText;
                this.GameHint = gameHint;
            }
        }

        [DebuggerDisplay("{GameText} || {GameHint}")]
        public class StringDescription : AchievementTableEntryDescription
        {
            public StringDescription(string? gameText = null, string? gameHint = null)
                : base(gameText, gameHint)
            {
            }
        }

        [DebuggerDisplay("Objectives: {EntryList.Count} || {GameText} || {GameHint}")]
        public class ObjectivesDescription : AchievementTableEntryDescription
        {
            public List<TableDescriptionEntry> EntryList { get; set; } = new List<TableDescriptionEntry>();

            public ObjectivesDescription(string? gameText = null, string? gameHint = null)
                : base(gameText, gameHint)
            {
            }
        }

        [DebuggerDisplay("CollectionItems: {EntryList.Count} || {GameText} || {GameHint}")]
        public class CollectionDescription : AchievementTableEntryDescription
        {
            public List<CollectionDescriptionEntry> EntryList { get; set; } = new List<CollectionDescriptionEntry>();

            public CollectionDescription(string? gameText = null, string? gameHint = null)
                : base(gameText, gameHint)
            {
            }

        }

        [DebuggerDisplay("{DisplayName}")]
        public class CollectionDescriptionEntry
        {
            public string DisplayName { get; set; } = string.Empty;

            public string ImageUrl { get; set; } = string.Empty;

            public string Link { get; set; } = string.Empty;
        }

        [DebuggerDisplay("{DisplayName}")]
        public class TableDescriptionEntry
        {
            public string DisplayName { get; set; } = string.Empty;

            public string Link { set; get; } = string.Empty;
        }
    }
}
