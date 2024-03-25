using EpubSharp;
using EpubSharp.Format;
using Microsoft.Playwright;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ComicSpider.Commands
{
    public class DownloadComic : AsyncCommand<GetCategories.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("Url to get list of categories")]
            [CommandArgument(0, "[url]")]
            public string? Url { get; set; }

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(Url))
                    return ValidationResult.Error("URL is required!!!");
                return ValidationResult.Success();
            }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, GetCategories.Settings settings)
        {
            EpubWriter writer = new EpubWriter();
            writer.AddFile("style.css", File.ReadAllText("./Resources/Css/Style.css"), EpubContentType.Css);
            writer.AddFile("arial.ttf", File.ReadAllBytes("./Resources/Fonts/Arial/arial.ttf"), EpubContentType.FontTruetype);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(settings.Url);

            var listChapterBarElement = await page.WaitForSelectorAsync("div#list-chapters:visible");

            if (listChapterBarElement != null)
            {
                await listChapterBarElement.ClickAsync();
                var listChaptersTableElement = await page.WaitForSelectorAsync("table#list-chapters:visible");
                var firstChapterRowElement = await page.WaitForSelectorAsync("table#list-chapters a.chapter:visible");

                if (firstChapterRowElement != null)
                {
                    await firstChapterRowElement.ClickAsync();
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]Chapters not found!!![/]");
                }
            }

            var chapterDetailElement = await page.WaitForSelectorAsync("div.chapter-detail");
            while (chapterDetailElement != null)
            {
                var buyChapterElement = await chapterDetailElement.QuerySelectorAsync("div.box-buy-chapter");

                if (buyChapterElement != null)
                    break;

                string chapterContent = string.Empty;

                var comicTitleElement = await chapterDetailElement.QuerySelectorAsync("ul.list-info li p");
                var comicTitle = await comicTitleElement.InnerTextAsync();

                var chapterTitleElement = await chapterDetailElement.QuerySelectorAsync("h1.chapter-title");
                var chapterTitle = await chapterTitleElement.InnerTextAsync();

                var chapterContentPartElements = await chapterDetailElement.QuerySelectorAllAsync(".webkit-chapter");

                chapterContent += $"<h2>{chapterTitle}</h2>";

                if (chapterContentPartElements != null && chapterContentPartElements.Count > 0)
                {
                    for (int i = 0; i < chapterContentPartElements.Count; i++)
                    {
                        var chapterContentPartElement = chapterContentPartElements[i];
                        var paragrapElements = await chapterContentPartElement.QuerySelectorAllAsync("p");
                        if (paragrapElements != null && paragrapElements.Count > 0)
                        {
                            var paragrapContent = new Dictionary<int, string>();
                            foreach (var paragrapElement in paragrapElements)
                            {
                                var paragrapOrder = await paragrapElement.EvaluateAsync<int>(@"(element) => {
                            return window.getComputedStyle(element).getPropertyValue('order');
                        }");
                                var paragrapText = string.Empty;
                                var childElements = await paragrapElement.QuerySelectorAllAsync("*");
                                if (childElements != null && childElements.Count > 0)
                                {
                                    var childContent = new Dictionary<int, string>();
                                    foreach (var childElement in childElements)
                                    {
                                        var childOrder = await childElement.EvaluateAsync<int>(@"(element) => {
                                    return window.getComputedStyle(element).getPropertyValue('order');
                                }");
                                        var childText = await childElement.InnerTextAsync();
                                        childContent[childOrder] = childText;
                                    }
                                    var childOrders = childContent.Keys.Order();
                                    foreach (var childOrder in childOrders)
                                    {
                                        paragrapText += childContent[childOrder];
                                    }
                                }
                                else
                                {
                                    paragrapText = await paragrapElement.InnerTextAsync();
                                }
                                paragrapContent[paragrapOrder] = paragrapText;
                            }
                            var orders = paragrapContent.Keys.Order();
                            foreach (var order in orders)
                            {
                                chapterContent += $"<p>{paragrapContent[order]}</p>";
                            }
                        }
                        else
                        {
                            var chapterContentPart = await chapterContentPartElement.InnerTextAsync();
                            chapterContent += $"<p>{chapterContentPart}</p>";
                        }
                    }
                }

                string chapterTemplate = File.ReadAllText("./Resources/Html/Template.html");

                writer.AddChapter(chapterTitle, string.Format(chapterTemplate, chapterTitle, chapterContent));

                var nextChapterButtonElement = await chapterDetailElement.QuerySelectorAsync("div.btn-control a:not(.disabled):has(.fas.fa-angle-right)");

                if (nextChapterButtonElement == null)
                    break;

                await nextChapterButtonElement.ClickAsync();
                chapterDetailElement = await page.WaitForSelectorAsync("div.chapter-detail");
            }
            writer.Write($"comic.epub");
            AnsiConsole.MarkupLine("[bold blue]DOWNLOAD DONE.[/]");
            return 0;
        }
    }
}
