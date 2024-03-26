using ComicSpider.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ComicSpider.Commands
{
    public class DownloadComic : AsyncCommand<DownloadComic.Settings>
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
            var downloaders = new List<IComicDowloader>()
            {
                new BNSComicDownloader()
            };
            var output = new EpubComicOutput();
            var downloadManager = new DownloadManager(downloaders, output);

            await downloadManager.InitializeAsync();

            await downloadManager.GetChaptersAsync(settings.Url);

            return 0;
        }
    }
}
