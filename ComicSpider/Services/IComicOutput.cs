using ComicSpider.Models;

namespace ComicSpider.Services
{
    public interface IComicOutput
    {
        void SaveCategories(List<Category> categories);

        void SaveComics(List<Comic> comics);

        void SaveChapters(List<Chapter> chapters);
    }
}
