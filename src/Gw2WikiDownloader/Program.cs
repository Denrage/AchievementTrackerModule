// See https://aka.ms/new-console-template for more information
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Libs.Interfaces;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.Json;

var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36";

Console.WriteLine("Hello, World!");


// Parse Achievement overview pages

//var httpClient = new HttpClient();
//httpClient.DefaultRequestHeaders.Add("user-agent", userAgent);
//var request = new HttpRequestMessage(HttpMethod.Get, "https://wiki.guildwars2.com/index.php?title=Category:Achievement_categories");
//var response = await httpClient.SendAsync(request);
//var secondPageAchievementsList = "<h2>Pages in category \"Achievement categories\"</h2>\n<p>The following 6 pages are in this category, out of 206 total.\n</p>(<a href=\"/index.php?title=Category:Achievement_categories&amp;pageuntil=World+vs+World+Warclaw#mw-pages\" title=\"Category:Achievement categories\">previous page</a>) (next page)<div lang=\"en\" dir=\"ltr\" class=\"mw-content-ltr\"><h3>W</h3>\n<ul>\n<li><a href=\"/wiki/World_vs_World_Warclaw\" title=\"World vs World Warclaw\">World vs World Warclaw</a></li>\n<li><a href=\"/wiki/Wrath_of_the_Twisted_Marionette\" title=\"Wrath of the Twisted Marionette\">Wrath of the Twisted Marionette</a></li></ul><h3>Y</h3>\n<ul>\n<li><a href=\"/wiki/Year_of_the_Ascension_Part_I\" title=\"Year of the Ascension Part I\">Year of the Ascension Part I</a></li>\n<li><a href=\"/wiki/Year_of_the_Ascension_Part_II\" title=\"Year of the Ascension Part II\">Year of the Ascension Part II</a></li>\n<li><a href=\"/wiki/Year_of_the_Ascension_Part_III\" title=\"Year of the Ascension Part III\">Year of the Ascension Part III</a></li>\n<li><a href=\"/wiki/Year_of_the_Ascension_Part_IV\" title=\"Year of the Ascension Part IV\">Year of the Ascension Part IV</a></li></ul></div>(<a href=\"/index.php?title=Category:Achievement_categories&amp;pageuntil=World+vs+World+Warclaw#mw-pages\" title=\"Category:Achievement categories\">previous page</a>) (next page)\n</div></div></div><div class=\"printfooter\">";
//var parser = new Gw2WikiDownload.WikiParser();

////var document = new HtmlAgilityPack.HtmlDocument();
////document.LoadHtml(File.ReadAllText("Untitled-1.html"));
////var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
////var result = parser.Parse(document, tables.First());

//var listElements = parser.ParseListElementsFromWiki(await response.Content.ReadAsStringAsync()).Skip(4).ToList();
//listElements.AddRange(parser.ParseListElementsFromWiki(secondPageAchievementsList));
//var result = new List<AchievementTableEntry>();
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

//async Task ParseItem(string name, string link, Action<int> setId, Func<bool> shouldSet)
//{
//    try
//    {
//        if (shouldSet())
//        {
//            Debug.WriteLine(name);
//            var web = new HtmlWeb();
//            var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + link);
//            var itemBoxNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'infobox')]").FindFirst("div");
//            setId(parser.ParseItemWikiPage(itemBoxNode));
//        }
//    }
//    catch (Exception)
//    {
//        throw;
//    }
//}

//foreach (var item in result)
//{
//    if (item.Description is CollectionDescription collectionDescription)
//    {
//        foreach (var collectionEntry in collectionDescription.EntryList)
//        {
//            await ParseItem(collectionEntry.DisplayName, collectionEntry.Link, id => collectionEntry.Id = id, () => collectionEntry.Id == 0);
//        }
//    }
//}

//foreach (var item in result)
//{
//    if (item.Reward is ItemReward reward)
//    {
//        await ParseItem(reward.DisplayName, reward.ItemUrl, id => reward.Id = id, () => reward.Id == 0);
//    }
//}

//var jsonResult = System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
//});

//File.WriteAllText("AchievementData_final2.json", jsonResult);










// Parse CollectionAchievement Details page 

//var result = System.Text.Json.JsonSerializer.Deserialize<List<Gw2WikiDownload.AchievementTableEntry>>(File.ReadAllText("AchievementData_final.json"), new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new Gw2WikiDownload.RewardConverter(), new Gw2WikiDownload.AchievementTableEntryDescriptionConverter() },
//});

//var collectionAchievementSource = File.ReadAllText("CollectionAchievementSource.html");
//var document = new HtmlDocument();
//document.LoadHtml(collectionAchievementSource);
//parser.ParseCollectionAchievementPage(document.DocumentNode);

//var achievementTables = new List<Gw2WikiDownload.CollectionAchievementTable>();
//foreach (var item in result)
//{
//    if (item.HasLink)
//    {
//        Debug.Write("Achievement: " + "https://wiki.guildwars2.com" + item.Link);
//        if (item.Link != "/wiki/Dragon_Response_Mission" && item.Link != "/wiki/Strike_Mission")
//        {
//            var web = new HtmlWeb();
//            var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.Link);
//            var table = parser.ParseCollectionAchievementPage(document.DocumentNode);
//            if (table != null)
//            {
//                table.Id = item.Id;
//                table.Name = item.Name;
//                table.Link = item.Link;
//                Debug.Write(" | Had table");
//                achievementTables.Add(table);
//            }
//        }

//        Debug.WriteLine("");
//    }
//}

//var json = System.Text.Json.JsonSerializer.Serialize(achievementTables, new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new Gw2WikiDownload.9CollectionAchievementTableEntryConverter() },
//});

//File.WriteAllText("achievementTables.json", json);


// Parse Subpages

//var result = System.Text.Json.JsonSerializer.Deserialize<List<AchievementTableEntry>>(File.ReadAllText("AchievementData_final2.json"), new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
//});

//var parser = new Gw2WikiDownload.WikiParser();
//var subpageInformation = new List<SubPageInformation>();
//await parser.ParseSubPage("https://wiki.guildwars2.com/wiki/Chasing_Tales:_Azra_the_Sunslayer", 0, subpageInformation);

//foreach (var item in result)
//{
//    var node = HtmlNode.CreateNode("<div>" + item.Description.GameText + "</div>");
//    foreach (var linkNode in node.ChildNodes.Where(x => x.Name == "a"))
//    {
//        await parser.ParseSubPage("https://wiki.guildwars2.com/" + linkNode.GetAttributeValue("href", ""), 0, subpageInformation);
//    }

//    if (!string.IsNullOrEmpty(item.Link))
//    {
//        await parser.ParseSubPage("https://wiki.guildwars2.com" + item.Link, 0, subpageInformation);
//    }

//    if (item.Description is ObjectivesDescription objectivesDescription)
//    {
//        foreach (var objective in objectivesDescription.EntryList)
//        {
//            if (objective is ILinkEntry linkEntry)
//            {
//                if (!string.IsNullOrEmpty(linkEntry.Link))
//                {
//                    await parser.ParseSubPage("https://wiki.guildwars2.com" + linkEntry.Link, 0, subpageInformation);
//                }
//            }
//        }
//    }

//    if (item.Description is CollectionDescription collectionDescription)
//    {
//        foreach (var collectionItem in collectionDescription.EntryList)
//        {
//            if (collectionItem is ILinkEntry linkEntry)
//            {
//                if (!string.IsNullOrEmpty(linkEntry.Link))
//                {
//                    await parser.ParseSubPage("https://wiki.guildwars2.com" + linkEntry.Link, 0, subpageInformation);
//                }
//            }
//        }
//    }

//    Console.Write($"\t\t\t{subpageInformation.Count}");
//}

//var json = System.Text.Json.JsonSerializer.Serialize(subpageInformation, new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Converters = { new SubPageInformationConverter() },
//});

//File.WriteAllText("subPages.json", json);


var result = System.Text.Json.JsonSerializer.Deserialize<List<SubPageInformation>>(File.ReadAllText("subPages.json"), new JsonSerializerOptions()
{
    WriteIndented = true,
    Converters = { new SubPageInformationConverter() },
});

var differentNodes = new List<HtmlNode>();

void Add(HtmlNode node)
{
    if (!differentNodes.Select(x => x.Name).Contains(node.Name))
    {
        differentNodes.Add(node);
    }
}

foreach (var page in result)
{
    var node = HtmlNode.CreateNode("<div>" + page.Description + "</div");

    foreach (var childNode in node.ChildNodes)
    {
        Add(childNode);
    }

    if (page is ItemSubPageInformation itemSubPage)
    {
        node = HtmlNode.CreateNode("<div>" + itemSubPage.Acquisition + "</div");

        foreach (var childNode in node.ChildNodes)
        {
            Add(childNode);
        }
    }

    if (page is LocationSubPageInformation location)
    {
        node = HtmlNode.CreateNode("<div>" + location.Statistics + "</div");

        foreach (var childNode in node.ChildNodes)
        {
            Add(childNode);
        }
    }

    if (page is IHasDescriptionList descriptionList)
    {
        foreach (var item in descriptionList.DescriptionList)
        {
            node = HtmlNode.CreateNode("<div>" + item.Key + "</div");

            foreach (var childNode in node.ChildNodes)
            {
                Add(childNode);
            }

            node = HtmlNode.CreateNode("<div>" + item.Value + "</div");

            foreach (var childNode in node.ChildNodes)
            {
                Add(childNode);
            }
        }
    }
}

Console.WriteLine("Done");