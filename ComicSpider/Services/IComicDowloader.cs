using ComicSpider.Models;

namespace ComicSpider.Services
{
    public interface IComicDowloader
    {
        string Domain { get; }

        event EventHandler<DownloadEventArgs> ReportProgress;

        Task<List<Category>> GetCategoriesAsync(string url);

        Task<List<Comic>> GetComicsAsync(string url, int pageNumber, int countNumber);

        Task<List<Chapter>> GetChaptersAsync(string url, string username, string password);
    }
}
