using ComicSpider.Models;

namespace ComicSpider.Services
{
    public interface IComicOutput
    {
        void SaveCategories(List<Category> categories, string fileName);

        void SaveComics(List<Comic> comics, string fileName);

        void SaveChapters(List<Chapter> chapters, string fileName);
    }
}
