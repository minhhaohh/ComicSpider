using ComicSpider.Models;

namespace ComicSpider.Services
{
    public interface IComicDowloader
    {
        string Domain { get; }

        event EventHandler<DownloadEventArgs> ReportProgress;

        Task<List<Category>> GetCategoriesAsync(DownloadContext context, string url);

        Task<List<Comic>> GetComicsAsync(DownloadContext context, string url, int pageNumber, int countNumber);

        Task<List<Chapter>> GetChaptersAsync(DownloadContext context, string url, string username, string password);
    }
}
