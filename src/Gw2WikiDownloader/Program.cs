// See https://aka.ms/new-console-template for more information
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Libs.Interfaces;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.Json;

var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36";

Console.WriteLine("Hello, World!");


// Parse Achievement overview pages

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("user-agent", userAgent);
var parser = new Gw2WikiDownload.WikiParser();
var listElements = new List<(string link, string title)>();

//var web = new HtmlWeb();
//var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com/wiki/Legendary_Weapons_(achievements)");
//var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
//var result = parser.Parse(document, tables.First());
var nextPage = false;
var nextPageLink = "https://wiki.guildwars2.com/index.php?title=Category:Achievement_categories";
do
{
    var request = new HttpRequestMessage(HttpMethod.Get, nextPageLink);
    var response = await httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    listElements.AddRange(parser.ParseListElementsFromWiki(content));



    var nextPageElementStart = content.IndexOf("(<a href=\"/index");
    var nextPageElementEnd = content.IndexOf("</a>", nextPageElementStart);

    var nextPageElement = content.Substring(nextPageElementStart, nextPageElementEnd - nextPageElementStart);

    if (nextPageElement.Contains("next page"))
    {
        var nextPageUrlStart = nextPageElement.IndexOf("\"");
        var nextPageUrlEnd = nextPageElement.IndexOf("\"", nextPageUrlStart + 1);
        nextPageLink = "https://wiki.guildwars2.com" + nextPageElement.Substring(nextPageUrlStart + 1, nextPageUrlEnd - nextPageUrlStart - 1).Replace("&amp;", "&");
        nextPage = true;
    }
    else
    {
        nextPage = false;
    }


} while (nextPage);

var result = new List<AchievementTableEntry>();
foreach (var item in listElements)
{
    if (item.title == "Living World Dailies")
    {
        Console.WriteLine(item.title + " skipped");
        continue;
    }

    Console.WriteLine(item.title);
    var web = new HtmlWeb();
    var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.link);
    var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
    result.AddRange(parser.Parse(document, tables.First()));
}

async Task ParseItem(string name, string link, Action<int> setId, Func<bool> shouldSet)
{
    try
    {
        if (shouldSet())
        {
            if (link == "/wiki/Transmutation_Charge")
            {
                link = "/wiki/Transmutation_Charge_(item)";
            }

            Console.WriteLine(name);
            var web = new HtmlWeb();
            var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + link);
            var itemBoxNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'infobox')]").FindFirst("div");
            setId(parser.ParseItemWikiPage(itemBoxNode));
        }
    }
    catch (Exception)
    {
        throw;
    }
}

foreach (var item in result)
{
    if (item.Description is CollectionDescription collectionDescription)
    {
        foreach (var collectionEntry in collectionDescription.EntryList)
        {
            await ParseItem(collectionEntry.DisplayName, collectionEntry.Link, id => collectionEntry.Id = id, () => collectionEntry.Id == 0);
        }
    }
}

foreach (var item in result)
{
    if (item.Reward is ItemReward reward)
    {
        await ParseItem(reward.DisplayName, reward.ItemUrl, id => reward.Id = id, () => reward.Id == 0);
    }
}

var jsonResult = System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
});

File.WriteAllText("achievement_data.json", jsonResult);










// Parse CollectionAchievement Details page 

result = System.Text.Json.JsonSerializer.Deserialize<List<AchievementTableEntry>>(File.ReadAllText("achievement_data.json"), new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
});


//var webtemp = new HtmlWeb();
//var document = await webtemp.LoadFromWebAsync("https://wiki.guildwars2.com/wiki/Molten_Memorial");
//parser.ParseCollectionAchievementPage(document.DocumentNode);

var achievementTables = new List<CollectionAchievementTable>();
foreach (var item in result)
{
    if (item.HasLink)
    {
        Console.Write("Achievement: " + "https://wiki.guildwars2.com" + item.Link);
        if (item.Link != "/wiki/Dragon_Response_Mission" && item.Link != "/wiki/Strike_Mission")
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.Link);
            var table = parser.ParseCollectionAchievementPage(doc.DocumentNode);
            if (table != null)
            {
                table.Id = item.Id;
                table.Name = item.Name;
                table.Link = item.Link;
                Console.Write(" | Had table");
                achievementTables.Add(table);
            }
        }

        Console.WriteLine("");
    }
}

var json = System.Text.Json.JsonSerializer.Serialize(achievementTables, new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new CollectionAchievementTableEntryConverter() },
});

File.WriteAllText("achievement_tables.json", json);


// Parse Subpages

result = System.Text.Json.JsonSerializer.Deserialize<List<AchievementTableEntry>>(File.ReadAllText("achievement_data.json"), new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
});

//var parser = new Gw2WikiDownload.WikiParser();
var subpageInformation = new List<SubPageInformation>();
//await parser.ParseSubPage("https://wiki.guildwars2.com/wiki/Chasing_Tales:_Azra_the_Sunslayer", 0, subpageInformation);

foreach (var item in result)
{
    var node = HtmlNode.CreateNode("<div>" + item.Description.GameText + "</div>");
    foreach (var linkNode in node.ChildNodes.Where(x => x.Name == "a"))
    {
        await parser.ParseSubPage("https://wiki.guildwars2.com/" + linkNode.GetAttributeValue("href", ""), 0, subpageInformation);
    }

    if (!string.IsNullOrEmpty(item.Link))
    {
        await parser.ParseSubPage("https://wiki.guildwars2.com" + item.Link, 0, subpageInformation);
    }

    if (item.Description is ObjectivesDescription objectivesDescription)
    {
        foreach (var objective in objectivesDescription.EntryList)
        {
            if (objective is ILinkEntry linkEntry)
            {
                if (!string.IsNullOrEmpty(linkEntry.Link))
                {
                    await parser.ParseSubPage("https://wiki.guildwars2.com" + linkEntry.Link, 0, subpageInformation);
                }
            }
        }
    }

    if (item.Description is CollectionDescription collectionDescription)
    {
        foreach (var collectionItem in collectionDescription.EntryList)
        {
            if (collectionItem is ILinkEntry linkEntry)
            {
                if (!string.IsNullOrEmpty(linkEntry.Link))
                {
                    await parser.ParseSubPage("https://wiki.guildwars2.com" + linkEntry.Link, 0, subpageInformation);
                }
            }
        }
    }

    Console.Write($"\t\t\t{subpageInformation.Count}");
}

jsonResult = System.Text.Json.JsonSerializer.Serialize(subpageInformation, new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new SubPageInformationConverter() },
});

File.WriteAllText("subPages.json", jsonResult);


//var result = System.Text.Json.JsonSerializer.Deserialize<List<SubPageInformation>>(File.ReadAllText("subPages.json"), new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new SubPageInformationConverter() },
//});

//var differentNodes = new List<HtmlNode>();

//void Add(HtmlNode node)
//{
//    if (!differentNodes.Select(x => x.Name).Contains(node.Name))
//    {
//        differentNodes.Add(node);
//    }
//}

//foreach (var page in result)
//{
//    var node = HtmlNode.CreateNode("<div>" + page.Description + "</div");

//    foreach (var childNode in node.ChildNodes)
//    {
//        Add(childNode);
//    }

//    if (page is ItemSubPageInformation itemSubPage)
//    {
//        node = HtmlNode.CreateNode("<div>" + itemSubPage.Acquisition + "</div");

//        foreach (var childNode in node.ChildNodes)
//        {
//            Add(childNode);
//        }
//    }

//    if (page is LocationSubPageInformation location)
//    {
//        node = HtmlNode.CreateNode("<div>" + location.Statistics + "</div");

//        foreach (var childNode in node.ChildNodes)
//        {
//            Add(childNode);
//        }
//    }

//    if (page is IHasDescriptionList descriptionList)
//    {
//        foreach (var item in descriptionList.DescriptionList)
//        {
//            node = HtmlNode.CreateNode("<div>" + item.Key + "</div");

//            foreach (var childNode in node.ChildNodes)
//            {
//                Add(childNode);
//            }

//            node = HtmlNode.CreateNode("<div>" + item.Value + "</div");

//            foreach (var childNode in node.ChildNodes)
//            {
//                Add(childNode);
//            }
//        }
//    }
//}

Console.WriteLine("Done");