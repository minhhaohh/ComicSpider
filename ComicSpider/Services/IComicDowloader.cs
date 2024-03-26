using ComicSpider.Models;
using Spectre.Console;

namespace ComicSpider.Services
{
    public interface IComicDowloader
    {
        string Domain { get; }

        Task<List<Category>> GetCategoriesAsync(DownloadContext context, string url);

        Task<List<Comic>> GetComicsAsync(DownloadContext context, string url, int pageNumber, int countNumber);

        Task<List<Chapter>> GetChaptersAsync(DownloadContext context, string url);

        event EventHandler<DownloadEventArgs> ReportProgress;
    }
}
