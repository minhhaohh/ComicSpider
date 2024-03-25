using Microsoft.Playwright;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ComicSpider.Commands
{
    public class GetCategories : AsyncCommand<GetCategories.Settings>
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

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();
            await page.GotoAsync(settings.Url);

            var uri = new Uri(settings.Url);
            var host = uri.Host;

            var table = new Table()
            {
                Title = new TableTitle("LIST OF CATEGORIES", "bold green"),
                Border = TableBorder.Rounded
            };
            table.AddColumns(
                "[blue bold]No[/]",
                "[blue bold]Gategory[/]",
                "[blue bold]URL[/]"
                );

            var categoryButton = await page.QuerySelectorAsync("a.item-link[data-target='#allCategory']");
            if (categoryButton != null)
            {
                await categoryButton.ClickAsync();
                var categories = await page.QuerySelectorAllAsync("a.categories-item-modal.categories-modal[data-dismiss='modal']");
                for (int i = 0; i < categories.Count; i++)
                {
                    var title = (await categories[i].InnerTextAsync()).Trim();
                    var href = await categories[i].GetAttributeAsync("href");
                    table.AddRow((i + 1).ToString(), title, host + href);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]Category button not found!!![/]");
                return 0;
            }
            AnsiConsole.Write(table);
            return 0;
        }
    }
}
