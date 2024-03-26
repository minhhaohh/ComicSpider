using ComicSpider.Models;
using CsvHelper;
using EpubSharp;
using EpubSharp.Format;
using System.Globalization;

namespace ComicSpider.Services
{
    public class FileComicOutput : IComicOutput
    {
        public void SaveCategories(List<Category> categories)
        {
            using (var writer = new StreamWriter($"categories_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(categories);
            }
        }

        public void SaveComics(List<Comic> comics)
        {
            using (var writer = new StreamWriter($"comics_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(comics);
            }
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
            writer.Write($"comic_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.epub");
        }
    }
}
