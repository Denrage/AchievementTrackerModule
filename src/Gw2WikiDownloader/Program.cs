// See https://aka.ms/new-console-template for more information
using Denrage.AchievementTrackerModule.Libs.Achievement;
using Denrage.AchievementTrackerModule.Libs.Interfaces;
using HtmlAgilityPack;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Text.Json;

internal class Program
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36";

    private static readonly Gw2WikiDownload.WikiParser parser = new();

    private static async Task Main(string[] args)
    {

        Console.WriteLine("Hello, World!");
        await AnsiConsole.Progress()
            .HideCompleted(true)
            .Columns(new ProgressColumn[]
            {
                new PercentageColumn(),         // Percentage
                new ProgressBarColumn(),        // Progress bar
                new TaskDescriptionColumn(),    // Task description
            })
            .StartAsync(async context =>
            {
                //var overviewTask = context.AddTask("OverviewParsing");
                //overviewTask.IsIndeterminate = true;
                //var elements = await ParseOverviewPage();
                //overviewTask.IsIndeterminate = false;
                //overviewTask.Value = overviewTask.MaxValue;
                //overviewTask.StopTask();

                //var achievementParseTask = context.AddTask("AchievementParsing");
                //await ParseAchievementPage(elements, context, achievementParseTask);
                //achievementParseTask.StopTask();

                //var collectionParseTask = context.AddTask("CollectionParsing");
                //await ParseCollection(context, collectionParseTask);
                //collectionParseTask.StopTask();

                var subPageParseTask = context.AddTask("SubpageParsing");
                await ParseSubPages(context, subPageParseTask);
                subPageParseTask.StopTask();
            });
    }

    private static async Task<List<(string link, string title)>> ParseOverviewPage()
    {
        // Parse Achievement overview pages

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("user-agent", UserAgent);
        var listElements = new List<(string link, string title)>();
        var nextPageLink = "https://wiki.guildwars2.com/index.php?title=Category:Achievement_categories";

        //var web = new HtmlWeb();
        //var document = await web.LoadFromWebAsync("https://wiki.guildwars2.com/wiki/Legendary_Weapons_(achievements)");
        //var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
        //var result = parser.Parse(document, tables.First());
        bool nextPage;
        do
        {
            var request = new HttpRequestMessage(HttpMethod.Get, nextPageLink);
            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            listElements.AddRange(parser.ParseListElementsFromWiki(content));



            var nextPageElementStart = content.IndexOf("(<a href=\"/index");
            var nextPageElementEnd = content.IndexOf("</a>", nextPageElementStart);

            var nextPageElement = content[nextPageElementStart..nextPageElementEnd];

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

        return listElements;
    }

    private static async Task ParseAchievementPage(List<(string link, string title)> listElements, ProgressContext progressContext, ProgressTask progress)
    {
        var result = new ConcurrentBag<AchievementTableEntry>();
        progress.MaxValue = listElements.Count;
        Parallel.For(0, listElements.Count, i =>
        {
            var item = listElements[i];
            if (item.title != "Living World Dailies")
            {
                //Console.WriteLine(item.title + " skipped");
                //continue;

                progress.Description(Markup.Escape(item.title));
                var web = new HtmlWeb();
                var document = web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.link).Result;
                var tables = document.DocumentNode.SelectNodes("//table[contains(@class, 'table')]");
                var elements = parser.Parse(document, tables.First(), progressContext);
                foreach (var element in elements)
                {
                    result.Add(element);
                }
            }
            progress.Increment(1);
        });

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

                    if (link == "/wiki/Essence_of_Luck")
                    {
                        link = "/wiki/Essence_of_Luck_(fine)";
                    }

                    if (link == "/wiki/Unbound_Magic")
                    {
                        link = "/wiki/Unbound_Magic_(service)";
                    }

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
        progress.Value = 0;
        progress.MaxValue = result.Count + result.Select(x => x.Description).OfType<CollectionDescription>().SelectMany(x => x.EntryList).Count();
        progress.Description = "Parsing Items ...";
        Parallel.ForEach(result, new ParallelOptions() { MaxDegreeOfParallelism = 15 }, item =>
        {
            var task = progressContext.AddTask("Item Download");
            task.IsIndeterminate = true;
            if (item.Description is CollectionDescription collectionDescription)
            {
                foreach (var collectionEntry in collectionDescription.EntryList)
                {
                    task.Description = Markup.Escape(collectionEntry.DisplayName);
                    ParseItem(collectionEntry.DisplayName, collectionEntry.Link, id => collectionEntry.Id = id, () => collectionEntry.Id == 0).Wait();
                    progress.Increment(1);
                }
            }
            else if (item.Reward is ItemReward reward)
            {
                task.Description = Markup.Escape(reward.DisplayName);
                ParseItem(reward.DisplayName, reward.ItemUrl, id => reward.Id = id, () => reward.Id == 0).Wait();
            }
            task.StopTask();
            progress.Increment(1);
            progress.Description = $"Parsing Items ({progress.Value}/{progress.MaxValue})";
        });

        var jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
        });
        File.WriteAllText("achievement_data.json", jsonResult);

    }

    private static async Task ParseCollection(ProgressContext progressContext, ProgressTask progress)
    {
        // Parse CollectionAchievement Details page 

        var result = JsonSerializer.Deserialize<List<AchievementTableEntry>>(File.ReadAllText("achievement_data.json"), new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
        });


        //var webtemp = new HtmlWeb();
        //var document = await webtemp.LoadFromWebAsync("https://wiki.guildwars2.com/wiki/Molten_Memorial");
        //parser.ParseCollectionAchievementPage(document.DocumentNode);

        var achievementTables = new ConcurrentBag<CollectionAchievementTable>();

        progress.Description = "Parsing Collections";
        progress.Value = 0;
        progress.MaxValue = result.Count;

        Parallel.For(0, result.Count, new ParallelOptions() { MaxDegreeOfParallelism = 15 }, i =>
        {
            var item = result[i];
            if (item.HasLink)
            {
                var task = progressContext.AddTask(item.Link);
                task.IsIndeterminate = true;
                if (item.Link != "/wiki/Dragon_Response_Mission" && item.Link != "/wiki/Strike_Mission")
                {
                    var web = new HtmlWeb();
                    var doc = web.LoadFromWebAsync("https://wiki.guildwars2.com" + item.Link).Result;
                    var table = parser.ParseCollectionAchievementPage(doc.DocumentNode);
                    if (table != null)
                    {
                        table.Id = item.Id;
                        table.Name = item.Name;
                        table.Link = item.Link;
                        achievementTables.Add(table);
                    }
                }

                task.StopTask();
            }

            progress.Increment(1);
            progress.Description = $"Parsing Collections ({progress.Value}/{progress.MaxValue})";

        });

        var json = JsonSerializer.Serialize(achievementTables, new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new CollectionAchievementTableEntryConverter() },
        });

        File.WriteAllText("achievement_tables.json", json);

    }

    private static async Task ParseSubPages(ProgressContext progressContext, ProgressTask progress)
    {

        // Parse Subpages

        var result = JsonSerializer.Deserialize<List<AchievementTableEntry>>(File.ReadAllText("achievement_data.json"), new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new RewardConverter(), new AchievementTableEntryDescriptionConverter() },
        });

        //var parser = new Gw2WikiDownload.WikiParser();
        var subpageInformation = new ConcurrentBag<SubPageInformation>();
        //await parser.ParseSubPage("https://wiki.guildwars2.com/wiki/Chasing_Tales:_Azra_the_Sunslayer", 0, subpageInformation);

        progress.Description = "Parsing SubPages";
        progress.Value = 0;
        progress.MaxValue = result.Count;

        Parallel.For(0, result.Count, new ParallelOptions() { MaxDegreeOfParallelism = 15 }, i =>
        {
            var item = result[i];
            var node = HtmlNode.CreateNode("<div>" + item.Description.GameText + "</div>");
            Parallel.ForEach(node.ChildNodes.Where(x => x.Name == "a"), linkNode =>
            {
                var task = progressContext.AddTask(linkNode.GetAttributeValue("href", "empty"));
                task.IsIndeterminate = true;
                parser.ParseSubPage("https://wiki.guildwars2.com/" + linkNode.GetAttributeValue("href", ""), 0, subpageInformation, task).Wait();
                task.StopTask();
            });

            if (!string.IsNullOrEmpty(item.Link))
            {
                var task = progressContext.AddTask(string.IsNullOrWhiteSpace(item.Link) ? string.Empty : item.Link);
                task.IsIndeterminate = true;
                parser.ParseSubPage("https://wiki.guildwars2.com" + item.Link, 0, subpageInformation, task).Wait();
                task.StopTask();
            }

            if (item.Description is ObjectivesDescription objectivesDescription)
            {
                foreach (var objective in objectivesDescription.EntryList)
                {
                    if (objective is ILinkEntry linkEntry)
                    {
                        if (!string.IsNullOrEmpty(linkEntry.Link))
                        {
                            var task = progressContext.AddTask(string.IsNullOrWhiteSpace(linkEntry.Link) ? string.Empty : linkEntry.Link);
                            task.IsIndeterminate = true;
                            parser.ParseSubPage("https://wiki.guildwars2.com" + linkEntry.Link, 0, subpageInformation, task).Wait();
                            task.StopTask();
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
                            var task = progressContext.AddTask(string.IsNullOrWhiteSpace(linkEntry.Link) ? string.Empty : linkEntry.Link);
                            task.IsIndeterminate = true;
                            parser.ParseSubPage("https://wiki.guildwars2.com" + linkEntry.Link, 0, subpageInformation, task).Wait();
                            task.StopTask();
                        }
                    }
                }
            }

            progress.Increment(1);
            progress.Description = $"Parsing SubPages ({progress.Value}/{progress.MaxValue})";
        });

        var jsonResult = JsonSerializer.Serialize(subpageInformation, new JsonSerializerOptions()
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
    }
}