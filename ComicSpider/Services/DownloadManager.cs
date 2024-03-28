using ComicSpider.Models;
using Microsoft.Playwright;
using Spectre.Console;

namespace ComicSpider.Services
{
    public class DownloadManager
    {
        private IComicDowloader _downloader;
        private IComicOutput _output;
        private IBrowser _browser;
        private ProgressTask _progressTask;

        public DownloadManager(IComicDowloader downloader, IComicOutput output)
        {
            _downloader = downloader;
            _output = output;
            var playwright = Playwright.CreateAsync().Result;
            _browser = playwright.Chromium.LaunchAsync(new() { Headless = false }).Result;
        }

        //public async Task InitializeAsync()
        //{
        //    var playwright = await Playwright.CreateAsync();
        //    _browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        //}

        public async Task GetCategoriesAsync(string url, string fileName)
        {
            DownloadContext context = new DownloadContext()
            {
                Page = await _browser.NewPageAsync(),
            };
            //var downloader = GetComicDowloader(url);
            _downloader.ReportProgress += Downloader_ReportProgress;
            await AnsiConsole.Progress()
               .StartAsync(async ctx =>
               {
                   _progressTask = ctx.AddTask("[green bold]Progress: [/]");
                   var categories = await _downloader.GetCategoriesAsync(context, url);
                   _output.SaveCategories(categories, fileName);
               });
        }

        public async Task GetComicsAsync(string url, int pageNumber, int countNumber, string fileName)
        {
            DownloadContext context = new DownloadContext()
            {
                Page = await _browser.NewPageAsync(),
            };
            //var downloader = GetComicDowloader(url);
            _downloader.ReportProgress += Downloader_ReportProgress;
            await AnsiConsole.Progress()
              .StartAsync(async ctx =>
              {
                  _progressTask = ctx.AddTask("[green bold]Progress: [/]");
                  var comics = await _downloader.GetComicsAsync(context, url, pageNumber, countNumber);
                  _output.SaveComics(comics, fileName);
              });
        }

        public async Task GetChaptersAsync(string url, string fileName)
        {
            DownloadContext context = new DownloadContext()
            {
                Page = await _browser.NewPageAsync(),
            };
            //var downloader = GetComicDowloader(url);
            _downloader.ReportProgress += Downloader_ReportProgress;
            await AnsiConsole.Progress()
              .StartAsync(async ctx =>
              {
                  _progressTask = ctx.AddTask("[green bold]Progress: [/]");
                  var chapters = await _downloader.GetChaptersAsync(context, url);
                  _output.SaveChapters(chapters, fileName);
              });
        }

        //private IComicDowloader GetComicDowloader(string url)
        //{
        //    var uri = new Uri(url);
        //    var host = uri.Host;
        //    return _downloaders.FirstOrDefault(a => a.Domain == host);
        //}

        void Downloader_ReportProgress(object sender, DownloadEventArgs e)
        {
            _progressTask.Value(e.Progress);
        }
    }
}
