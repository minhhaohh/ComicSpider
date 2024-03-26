using ComicSpider.Services;
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

            [Description("File name to export .csv")]
            [CommandOption("--file")]
            public string? FileName { get; set; }

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
            var downloaders = new List<IComicDowloader>()
            {
                new BNSComicDownloader()
            };
            IComicOutput output = settings.FileName == null ? new ConsoleComicOutput() : new FileComicOutput();
            var downloadManager = new DownloadManager(downloaders, output);

            await downloadManager.InitializeAsync();

            await downloadManager.GetComicsAsync(settings.Url, settings.Page, settings.Count, settings.FileName);

            return 0;
        }
    }
}
