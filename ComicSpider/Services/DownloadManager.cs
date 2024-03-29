using ComicSpider.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ComicSpider.Services
{
    public class DownloadManager
    {
        private readonly ILogger<DownloadManager> _logger;

        private readonly IComicDowloader _downloader;
        
        private readonly IComicOutput _output;

        private ProgressTask _progressTask;

        public DownloadManager(ILogger<DownloadManager> logger, IComicDowloader downloader, IComicOutput output)
        {
            _logger = logger;
            _downloader = downloader;
            _output = output;
        }

        public async Task GetCategoriesAsync(string url, string fileName)
        {
            try
            {
                _downloader.ReportProgress += Downloader_ReportProgress;
                await AnsiConsole.Progress()
                   .StartAsync(async ctx =>
                   {
                       _progressTask = ctx.AddTask("[green bold]Progress: [/]");
                       var categories = await _downloader.GetCategoriesAsync(url);
                       if (categories == null || categories.Count == 0)
                       {
                           _logger.LogWarning("No data categories found!!!");
                           return;
                       }
                       _output.SaveCategories(categories, fileName);
                   });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            
        }

        public async Task GetComicsAsync(string url, int pageNumber, int countNumber, string fileName)
        {
            try
            {
                _downloader.ReportProgress += Downloader_ReportProgress;
                await AnsiConsole.Progress()
                  .StartAsync(async ctx =>
                  {
                      _progressTask = ctx.AddTask("[green bold]Progress: [/]");
                      var comics = await _downloader.GetComicsAsync(url, pageNumber, countNumber);
                      if (comics == null || comics.Count == 0)
                      {
                          _logger.LogWarning("No data comics found!!!");
                          return;
                      }
                      _output.SaveComics(comics, fileName);
                  });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
        }

        public async Task GetChaptersAsync(string url, string fileName, string username, string password)
        {
            try
            {
                _downloader.ReportProgress += Downloader_ReportProgress;
                await AnsiConsole.Progress()
                  .StartAsync(async ctx =>
                  {
                      _progressTask = ctx.AddTask("[green bold]Progress: [/]");
                      var chapters = await _downloader.GetChaptersAsync(url, username, password);
                      if (chapters == null || chapters.Count == 0)
                      {
                          _logger.LogWarning("No data chapters comic found!!!");
                          return;
                      }
                      _output.SaveChapters(chapters, fileName);
                  });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
        }

        void Downloader_ReportProgress(object sender, DownloadEventArgs e)
        {
            _progressTask.Value(e.Progress);
        }
    }
}
