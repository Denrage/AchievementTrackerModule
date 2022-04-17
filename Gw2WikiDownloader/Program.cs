// See https://aka.ms/new-console-template for more information
using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

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
//var listElements = parser.ParseListElementsFromWiki(await response.Content.ReadAsStringAsync()).Skip(4);
//var result = new List<Gw2WikiDownload.WikiParser.AchievementTableEntry>();
//foreach (var item in listElements)
//{
//    if (item.title == "Living World Dailies")
//    {
//        System.Diagnostics.Debug.WriteLine(item.title + " skipped");
//        continue;
//    }
//    System.Diagnostics.Debug.WriteLine(item.title);
//    var web = new HtmlWeb();
//    var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.link);
//    var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
//    result.AddRange(parser.Parse(document, tables.First()));
//}


//var jsonResult = System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new Gw2WikiDownload.WikiParser.RewardConverter(), new Gw2WikiDownload.WikiParser.AchievementTableEntryDescriptionConverter() },
//});

//File.WriteAllText("AchievementData.json", jsonResult);

var result = JsonSerializer.Deserialize<List<Gw2WikiDownload.WikiParser.AchievementTableEntry>>(File.ReadAllText("AchievementData.json"), new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new Gw2WikiDownload.WikiParser.RewardConverter(), new Gw2WikiDownload.WikiParser.AchievementTableEntryDescriptionConverter() },
});
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
                    var entry = new AchievementTableEntry()
                    {
                        Name = achievementName.Name,
                        Link = achievementName.Link,
                    };
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
                            var (rewardImage, rewardName, rewardLink) = ParseItemReward(tableData[2]);
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
            catch (Exception ex)
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

            public string? Link { get; set; } = null;

            public bool HasLink => this.Link != null;

            public string? Prerequisite { get; set; } = null;

            public AchievementTitle Title { get; set; } = AchievementTitle.EmptyTitle;

            public Reward Reward { get; set; } = Reward.EmptyReward;

            public AchievementTableEntryDescription? Description { get; set; } = null;

            public string? Cite { get; set; } = null;
        }

        public class RewardConverter : JsonConverter<Reward>
        {
            private const string TypeValuePropertyName = "TypeValue";
            public override bool CanConvert(Type typeToConvert)
                => typeof(Reward).IsAssignableFrom(typeToConvert);

            public override Reward? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != nameof(TypeDiscriminator))
                {
                    throw new JsonException();
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                {
                    throw new JsonException();
                }

                static Reward ParseReward<T>(ref Utf8JsonReader reader)
                    where T : Reward
                {
                    if (!reader.Read() || reader.GetString() != TypeValuePropertyName)
                    {
                        throw new JsonException();
                    }
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }

                    var result = (T?)JsonSerializer.Deserialize(ref reader, typeof(T));

                    if (result is null)
                    {
                        throw new JsonException();
                    }

                    return result;
                }

                Reward reward;
                TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
                switch (typeDiscriminator)
                {
                    case TypeDiscriminator.EmptyReward:
                        reward = ParseReward<EmptyReward>(ref reader);
                        break;
                    case TypeDiscriminator.MultiTierReward:
                        reward = ParseReward<MultiTierReward>(ref reader);
                        break;
                    case TypeDiscriminator.ItemReward:
                        reward = ParseReward<ItemReward>(ref reader);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                return reward;
            }

            public override void Write(Utf8JsonWriter writer, Reward value, JsonSerializerOptions options)
            {
                void WriteTypeDiscriminator<T>(Utf8JsonWriter writer, T reward, TypeDiscriminator typeDiscriminator)
                    where T : Reward
                {

                    writer.WriteNumber(nameof(TypeDiscriminator), (int)typeDiscriminator);
                    writer.WritePropertyName(TypeValuePropertyName);
                    JsonSerializer.Serialize(writer, reward);
                }

                writer.WriteStartObject();

                switch (value)
                {
                    case MultiTierReward multiTierReward:
                        WriteTypeDiscriminator(writer, multiTierReward, TypeDiscriminator.MultiTierReward);
                        break;
                    case ItemReward itemReward:
                        WriteTypeDiscriminator(writer, itemReward, TypeDiscriminator.ItemReward);
                        break;
                    case EmptyReward emptyReward:
                        WriteTypeDiscriminator(writer, emptyReward, TypeDiscriminator.EmptyReward);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                writer.WriteEndObject();
            }

            private enum TypeDiscriminator
            {
                EmptyReward = 0,
                ItemReward = 1,
                MultiTierReward = 2,
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
            public string ImageUrl { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public string ItemUrl { get; set; } = string.Empty;
        }

        [DebuggerDisplay("{Tiers[0].DisplayName}")]
        public class MultiTierReward : Reward
        {
            public List<TierReward> Tiers { get; set; } = new List<TierReward>();

            public class TierReward : ItemReward
            {
                public int Tier { get; set; } = default;
            }
        }

        [DebuggerDisplay("{Title}")]
        public class AchievementTitle
        {
            public static AchievementTitle EmptyTitle { get; } = new AchievementTitle() { Title = string.Empty };

            public string Title { get; set; } = string.Empty;
        }


        public class AchievementTableEntryDescriptionConverter : JsonConverter<AchievementTableEntryDescription>
        {
            private const string TypeValuePropertyName = "TypeValue";
            public override bool CanConvert(Type typeToConvert)
                => typeof(AchievementTableEntryDescription).IsAssignableFrom(typeToConvert);

            public override AchievementTableEntryDescription? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != nameof(TypeDiscriminator))
                {
                    throw new JsonException();
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.Number)
                {
                    throw new JsonException();
                }

                static AchievementTableEntryDescription ParseDescription<T>(ref Utf8JsonReader reader)
                    where T : AchievementTableEntryDescription
                {
                    if (!reader.Read() || reader.GetString() != TypeValuePropertyName)
                    {
                        throw new JsonException();
                    }
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }

                    var result = (T?)JsonSerializer.Deserialize(ref reader, typeof(T));

                    if (result is null)
                    {
                        throw new JsonException();
                    }

                    return result;
                }

                AchievementTableEntryDescription description;
                TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
                switch (typeDiscriminator)
                {
                    case TypeDiscriminator.String:
                        description = ParseDescription<StringDescription>(ref reader);
                        break;
                    case TypeDiscriminator.Objective:
                        description = ParseDescription<ObjectivesDescription>(ref reader);
                        break;
                    case TypeDiscriminator.Collection:
                        description = ParseDescription<CollectionDescription>(ref reader);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                return description;
            }

            public override void Write(Utf8JsonWriter writer, AchievementTableEntryDescription value, JsonSerializerOptions options)
            {
                void WriteTypeDiscriminator<T>(Utf8JsonWriter writer, T description, TypeDiscriminator typeDiscriminator)
                    where T: AchievementTableEntryDescription
                {
                    writer.WriteNumber(nameof(TypeDiscriminator), (int)typeDiscriminator);
                    writer.WritePropertyName(TypeValuePropertyName);
                    JsonSerializer.Serialize(writer, description);
                }

                writer.WriteStartObject();

                switch (value)
                {
                    case StringDescription stringDescription:
                        WriteTypeDiscriminator(writer, stringDescription, TypeDiscriminator.String);
                        break;
                    case ObjectivesDescription objectivesDescription:
                        WriteTypeDiscriminator(writer, objectivesDescription, TypeDiscriminator.Objective);
                        break;
                    case CollectionDescription collectionDescription:
                        WriteTypeDiscriminator(writer, collectionDescription, TypeDiscriminator.Collection);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                writer.WriteEndObject();
            }

            private enum TypeDiscriminator
            {
                String = 0,
                Objective = 1,
                Collection = 2,
            }
        }


        public abstract class AchievementTableEntryDescription
        {
            public string? GameText { get; set; } = null;

            public string? GameHint { get; set; } = null;
        }

        [DebuggerDisplay("{GameText} || {GameHint}")]
        public class StringDescription : AchievementTableEntryDescription
        {
        }

        [DebuggerDisplay("Objectives: {EntryList.Count} || {GameText} || {GameHint}")]
        public class ObjectivesDescription : AchievementTableEntryDescription
        {
            public List<TableDescriptionEntry> EntryList { get; set; } = new List<TableDescriptionEntry>();
        }

        [DebuggerDisplay("CollectionItems: {EntryList.Count} || {GameText} || {GameHint}")]
        public class CollectionDescription : AchievementTableEntryDescription
        {
            public List<CollectionDescriptionEntry> EntryList { get; set; } = new List<CollectionDescriptionEntry>();
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
