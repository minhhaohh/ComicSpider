using Microsoft.Playwright;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ComicSpider.Commands
{
    public class GetComics : AsyncCommand<GetComics.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [Description("Url to get list of comics")]
            [CommandArgument(0, "[url]")]
            public string? Url { get; set; }

            [Description("First page to get list of comics")]
            [CommandOption("--page")]
            [DefaultValue(1)]
            public int Page { get; set; }

            [Description("Total page to get list of comics")]
            [CommandOption("--count")]
            [DefaultValue(1)]
            public int Count { get; set; }

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(Url))
                    return ValidationResult.Error("URL is required!!!");
                if (Page <= 0 || Count <= 0)
                    return ValidationResult.Error("Page and count must be greater than 0!!!");
                return ValidationResult.Success();
            }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(settings.Url);
            await page.WaitForSelectorAsync("div.container");

            var uri = new Uri(settings.Url);

            var table = new Table()
            {
                Title = new TableTitle("LIST OF COMICS", "bold green"),
                Border = TableBorder.Rounded
            };
            table.AddColumns(
                "[blue bold]No[/]",
                "[blue bold]Title[/]",
                "[blue bold]Author[/]",
                "[blue bold]Total Chapter[/]",
                "[blue bold]URL[/]"
                );

            var total = 0;
            var index = 1;

            for (int currentPage = settings.Page; currentPage < settings.Page + settings.Count; currentPage++)
            {
                var pageNumberElements = await page.QuerySelectorAllAsync("li.vue-page-item");
                if (pageNumberElements != null && pageNumberElements.Count > 0)
                {
                    var lastPageNumber = await pageNumberElements[pageNumberElements.Count - 1].InnerTextAsync();
                    if (settings.Page + settings.Count - 1 > int.Parse(lastPageNumber))
                    {
                        AnsiConsole.MarkupLine($"[bold red]Error:[/] There are {lastPageNumber} pages total!!!");
                        return 0;
                    }

                    var pageNumberActiveElement = await page.QuerySelectorAsync("li.vue-page-item.active");
                    var pageNumberActive = await pageNumberActiveElement.InnerTextAsync();
                    if (currentPage != int.Parse(pageNumberActive))
                    {
                        var pageNumberChooserElement = await page.QuerySelectorAsync("li.vue-page-item.disabled.break-view-class");
                        if (pageNumberChooserElement != null)
                        {
                            await pageNumberChooserElement.ClickAsync();

                            var inputPageElement = await page.QuerySelectorAsync("div.box-option-paginate input");
                            await inputPageElement.FillAsync(settings.Page.ToString());

                            var buttonMoveElement = await page.QuerySelectorAsync("div.box-option-paginate button.btn.btn-primary");
                            await buttonMoveElement.ClickAsync();
                        }
                        else
                        {
                            foreach (var pageNumberElement in pageNumberElements)
                            {
                                var pageNumber = await pageNumberElement.InnerTextAsync();
                                if (currentPage == int.Parse(pageNumber))
                                {
                                    await pageNumberElement.ClickAsync();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (settings.Page != 1 || settings.Count != 1)
                    {
                        AnsiConsole.MarkupLine("[bold red]Error: [/]There is 1 page total!!!");
                        return 0;
                    }
                }

                var comicElements = await page.QuerySelectorAllAsync("div.novel-item");

                if (comicElements != null)
                {

                    for (int i = 0; i < comicElements.Count; i++)
                    {
                        var comicElement = comicElements[i];

                        var titleElement = await comicElement.QuerySelectorAsync("h4 a");
                        var title = (await titleElement.InnerTextAsync()).Trim();

                        var href = await titleElement.GetAttributeAsync("href");

                        var authorElement = await comicElement.QuerySelectorAsync("div.author-source a.author");
                        var author = (await authorElement.InnerTextAsync()).Trim();

                        var totalChapterElement = await comicElement.QuerySelectorAsync("div.story-info span");
                        var totalChapter = (await totalChapterElement.InnerTextAsync()).Trim();

                        table.AddRow(index.ToString(), title, author, totalChapter, "https://" + uri.Host + href);
                        total++;
                        index++;
                    }
                }
            }
            AnsiConsole.Write(table);
            return 0;
        }
    }
}
