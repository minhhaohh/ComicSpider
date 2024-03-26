using ComicSpider.Models;
using EpubSharp.Format;
using EpubSharp;

namespace ComicSpider.Services
{
    public class EpubComicOutput : IComicOutput
    {
        public void SaveCategories(List<Category> categories)
        {
            throw new NotImplementedException();
        }

        public void SaveComics(List<Comic> comics)
        {
            throw new NotImplementedException();
        }

        public void SaveChapters(List<Chapter> chapters)
        {
            EpubWriter writer = new EpubWriter();
            writer.AddFile("style.css", File.ReadAllText("./Resources/Css/Style.css"), EpubContentType.Css);
            writer.AddFile("arial.ttf", File.ReadAllBytes("./Resources/Fonts/Arial/arial.ttf"), EpubContentType.FontTruetype);

            foreach (var chapter in chapters)
            {
                string chapterContent = $"<h2>{chapter.Title}</h2>";

                foreach (var paragraph in chapter.Paragraphs)
                {
                    chapterContent += $"<p>{paragraph}</p>";
                }

                string chapterTemplate = File.ReadAllText("./Resources/Html/Template.html");

                writer.AddChapter(chapter.Title, string.Format(chapterTemplate, chapter.Title, chapterContent));
            }
            writer.Write($"comic.epub");
        }
    }
}
