using ComicSpider.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ComicSpider.Commands
{
    public class DownloadComicCommand : AsyncCommand<DownloadComicCommand.Settings>
    {
        private readonly DownloadManager _downloadManager;

        public DownloadComicCommand(DownloadManager downloadManager)
        {
            _downloadManager = downloadManager;
        }

        public sealed class Settings : CommandSettings
        {
            [Description("Url to get list of categories")]
            [CommandArgument(0, "[url]")]
            public string Url { get; set; }

            [Description("File name to export .epub")]
            [CommandOption("--file")]
            public string FileName { get; set; }

            [Description("Username to login page")]
            [CommandOption("--username")]
            public string Username { get; set; }

            [Description("Password to login page")]
            [CommandOption("--password")]
            public string Password { get; set; }

            public override ValidationResult Validate()
            {
                if (string.IsNullOrEmpty(Url))
                    return ValidationResult.Error("URL is required!!!");

                if ((string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)))
                    return ValidationResult.Error("Missing username!!!");

                if ((!string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)))
                    return ValidationResult.Error("Missing password!!!");

                return ValidationResult.Success();
            }
        }

        public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            await _downloadManager.GetChaptersAsync(settings.Url, settings.FileName, settings.Username, settings.Password);

            return 0;
        }
    }
}
