using ComicSpider.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ComicSpider.Commands
{
    public class GetCategoriesCommand : AsyncCommand<GetCategoriesCommand.Settings>
    {
        private readonly DownloadManager _downloadManager;

        public GetCategoriesCommand(DownloadManager downloadManager)
        {
            _downloadManager = downloadManager;
        }

        public sealed class Settings : CommandSettings
        {
            [Description("Url to get list of categories")]
            [CommandArgument(0, "[url]")]
            public string Url { get; set; }

            [Description("File name to export .csv")]
            [CommandOption("--file")]
            public string FileName { get; set; }

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(Url))
                    return ValidationResult.Error("URL is required!!!");
                return ValidationResult.Success();
            }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            await _downloadManager.InitializeAsync();
            await _downloadManager.GetCategoriesAsync(settings.Url, settings.FileName);

            return 0;
        }
    }
}
