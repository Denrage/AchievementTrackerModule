// See https://aka.ms/new-console-template for more information
using HtmlAgilityPack;
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

var document = new HtmlAgilityPack.HtmlDocument();
document.LoadHtml(File.ReadAllText("Untitled-1.html"));
var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");

var result = parser.Parse(tables.First().OuterHtml);
//var listElements = parser.ParseListElementsFromWiki(await response.Content.ReadAsStringAsync()).Skip(4);
//var result = new List<Gw2WikiDownload.WikiParser.AchievementTableEntry>();
//foreach (var item in listElements)
//{
//    if (item.title.Contains("Daily") || item.title == "Living World Dailies")
//    {
//        System.Diagnostics.Debug.WriteLine(item.title + " skipped");
//        continue;
//    }
//    System.Diagnostics.Debug.WriteLine(item.title);
//    var web = new HtmlWeb();
//    var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.link);
//    var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
//    result.AddRange(parser.Parse(tables.First().OuterHtml));
//}


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
        public IEnumerable<AchievementTableEntry> Parse(string table)
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(table);
            var groupedAchievements = new Dictionary<string, List<HtmlNode>>();
            var achievementNodes = document.DocumentNode.SelectNodes("//tr[@data-id]");
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
                if (achievement[1].InnerHtml.Contains("Objectives:"))
                {
                    result.Add(ParseAchievementTable(achievement));
                }
                else if (achievement[1].InnerHtml.Contains("-bit"))
                {
                    result.Add(ParseCollection(achievement));
                }
                else
                {
                    var achievementName = ParseAchievementName(achievement);
                    var descriptionCell = achievement[1].ChildNodes.FindFirst("td");
                    var gameText = SanitizesDisplayName(descriptionCell.ChildNodes[0].InnerText);
                    var gameHint = string.Empty;

                    if (descriptionCell.ChildNodes.FindFirst("span") != null)
                    {
                        gameHint = SanitizesDisplayName(descriptionCell.ChildNodes.FindFirst("span").InnerText);
                    }
                    var descriptionElementList = descriptionCell.ChildNodes.FindFirst("dl");
                    var reward = Reward.EmptyReward;
                    var prerequisite = string.Empty;

                    if (descriptionElementList != null)
                    {
                        var descriptionElements = descriptionElementList.ChildNodes.Where(x => x.Name == "dd");
                        if (descriptionElements.Any())
                        {
                            if (descriptionElements.First().InnerHtml.Contains("Prerequisite:"))
                            {
                                prerequisite = descriptionElements.First().LastChild.InnerText;
                            }

                            if (descriptionElements.Select(x => x.InnerHtml).Any(x => x.Contains("Reward:") || x.Contains("Title:")))
                            {
                                reward = ParseReward(descriptionElements);
                            }
                        }
                    }

                    var description = new StringDescription(gameText, prerequisite) { GameHint = gameHint };
                    description.Reward = reward;
                    result.Add(new AchievementTableEntry(achievementName.Name, description, achievementName.Link));
                }

            }

            return result;
        }

        private static (string Name, string Link) ParseAchievementName(List<HtmlNode> achievement)
        {
            try
            {
                var headerRow = achievement[0];
                var headerNameColumn = headerRow.ChildNodes.FindFirst("th");
                var possibleLinkNodes = headerNameColumn.ChildNodes.Where(x => x.Name == "a");

                if (possibleLinkNodes.Any())
                {
                    var linkNode = possibleLinkNodes.FirstOrDefault(x => x.ChildNodes.FindFirst("img") == null);
                    
                    if (linkNode != null)
                    {
                        return (SanitizesDisplayName(linkNode.InnerText), linkNode.GetAttributeValue("href", string.Empty));
                    }
                }

                return (SanitizesDisplayName(headerNameColumn.InnerText), string.Empty);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static HtmlNode GetDescription(List<HtmlNode> achievement)
            => achievement[1].ChildNodes.FindFirst("td");

        private static Reward ParseReward(IEnumerable<HtmlNode> descriptionElements)
        {
            var rewardDescriptionElement = descriptionElements.First();
            if (descriptionElements.First().InnerHtml.Contains("Prerequisite:"))
            {
                rewardDescriptionElement = descriptionElements.Skip(1).First();
            }

            // TODO: Parse multitier reward
            if (rewardDescriptionElement.InnerHtml.Contains("Reward:"))
            {
                var rewardImage = rewardDescriptionElement.ChildNodes.FindFirst("span").ChildNodes.FindFirst("a").ChildNodes.FindFirst("img").GetAttributeValue("src", string.Empty);
                var rewardNameAndLinkElement = rewardDescriptionElement.ChildNodes.FindFirst("a");
                var rewardName = rewardNameAndLinkElement.GetAttributeValue("title", string.Empty);
                var rewardLink = rewardNameAndLinkElement.GetAttributeValue("href", string.Empty);

                return new ItemReward(rewardImage, rewardName, rewardLink);
            }
            else if(rewardDescriptionElement.InnerHtml.Contains(""))
            {
                return new TitleReward(SanitizesDisplayName(rewardDescriptionElement.LastChild.InnerText));
            }
        }

        private static AchievementTableEntry ParseCollection(List<HtmlNode> achievement)
        {
            var descriptionCell = GetDescription(achievement);
            var descriptionElements = descriptionCell.ChildNodes.First(x => x.Name == "dl").ChildNodes.Where(x => x.Name == "dd");
            var prerequisite = string.Empty;

            if (descriptionElements.First().InnerHtml.Contains("Prerequisite:"))
            {
                prerequisite = descriptionElements.First().LastChild.InnerText;
            }

            var description = new CollectionDescription(SanitizesDisplayName(descriptionCell.ChildNodes[0].InnerText), ParseReward(descriptionElements), prerequisite);

            var descriptionCollectionElement = descriptionElements.First(x => x.InnerHtml.Contains("-bit"));

            foreach (var item in descriptionCollectionElement.ChildNodes.FindFirst("div").ChildNodes.Where(x => x.Name == "div"))
            {
                description.EntryList.Add(new CollectionDescriptionEntry() { DisplayName = SanitizesDisplayName(item.ChildNodes.FindFirst("a").GetAttributeValue("title", string.Empty)), Link = item.ChildNodes.FindFirst("a").GetAttributeValue("href", string.Empty), ImageUrl = item.ChildNodes.FindFirst("a").ChildNodes.FindFirst("img").GetAttributeValue("src", string.Empty) });
            }

            var achievementName = ParseAchievementName(achievement);
            return new AchievementTableEntry(achievementName.Name, description, achievementName.Link);
        }

        private static AchievementTableEntry ParseAchievementTable(List<HtmlNode> achievement)
        {
            var descriptionCell = GetDescription(achievement);
            var descriptionElements = descriptionCell.ChildNodes.First(x => x.Name == "dl").ChildNodes.Where(x => x.Name == "dd");
            var prerequisite = string.Empty;

            if (descriptionElements.First().InnerHtml.Contains("Prerequisite:"))
            {
                prerequisite = descriptionElements.First().LastChild.InnerText;
            }

            var description = new TableDescription(SanitizesDisplayName(descriptionCell.ChildNodes[0].InnerText), ParseReward(descriptionElements), prerequisite);

            // TODO: Better handling of multi tier rewards and finding correct table
            var descriptionTable = descriptionElements.First(x => x.ChildNodes.FindFirst("table") != null && !x.InnerHtml.Contains("Rewards")).ChildNodes.FindFirst("table");
            var tableBody = descriptionTable.ChildNodes.FindFirst("tbody");
            var tableRows = tableBody.ChildNodes.Where(x => x.Name == "tr" && !x.InnerHtml.Contains("Objectives:") && !x.InnerHtml.Contains("<b>..."));

            foreach (var row in tableRows)
            {
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

            var achievementName = ParseAchievementName(achievement);
            return new AchievementTableEntry(achievementName.Name, description, achievementName.Link);
        }

        private static string SanitizesDisplayName(string displayName)
            => string.Join(" ", WebUtility.HtmlDecode(displayName).Replace(Environment.NewLine, "").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        public class AchievementTableEntry
        {
            public string Title { get; set; }

            public AchievementTableEntryDescription Description { get; set; }

            public string Link { get; set; }

            public AchievementTableEntry(string title, AchievementTableEntryDescription description, string link = "")
            {
                this.Title = title;
                this.Description = description;
                this.Link = link;
            }
        }

        public interface IRewardable
        {
            Reward Reward { get; set; }
        }

        public abstract class Reward
        {
            public static Reward EmptyReward { get; } = new EmptyReward();
        }

        public class EmptyReward : Reward
        {

        }

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

        public class TitleReward : Reward
        {
            public string Title { get; set; }

            public TitleReward(string title)
            {
                this.Title = title;
            }
        }

        public abstract class AchievementTableEntryDescription
        {
            public string GameText { get; set; } = string.Empty;

            public string Prerequisite { get; set; }

            protected AchievementTableEntryDescription(string gameText, string prerequisite)
            {
                this.GameText = gameText;
                this.Prerequisite = prerequisite;
            }
        }

        public class StringDescription : AchievementTableEntryDescription, IRewardable
        {
            public StringDescription(string gameText, string prerequisite)
                : base(gameText, prerequisite)
            {
            }

            public string GameHint { get; set; } = string.Empty;

            public Reward Reward { get; set; } = Reward.EmptyReward;
        }

        public class TableDescription : AchievementTableEntryDescription, IRewardable
        {
            public Reward Reward { get; set; }

            public List<TableDescriptionEntry> EntryList { get; set; } = new List<TableDescriptionEntry>();

            public TableDescription(string gameText, Reward reward, string prerequisite)
                : base(gameText, prerequisite)
            {
                this.Reward = reward;
            }
        }

        public class CollectionDescription : AchievementTableEntryDescription, IRewardable
        {
            public CollectionDescription(string gameText, Reward reward, string prerequisite)
                : base(gameText, prerequisite)
            {
                this.Reward = reward;
            }

            public Reward Reward { get; set; }

            public List<CollectionDescriptionEntry> EntryList { get; set; } = new List<CollectionDescriptionEntry>();
        }

        public class CollectionDescriptionEntry
        {
            public string DisplayName { get; set; } = string.Empty;

            public string ImageUrl { get; set; } = string.Empty;

            public string Link { get; set; } = string.Empty;
        }

        public class TableDescriptionEntry
        {
            public string DisplayName { get; set; } = string.Empty;

            public string Link { set; get; } = string.Empty;
        }
    }
}
