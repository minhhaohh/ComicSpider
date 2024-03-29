using ComicSpider.Models;
using CsvHelper;
using EpubSharp;
using EpubSharp.Format;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ComicSpider.Services
{
    public class FileComicOutput : IComicOutput
    {
        private readonly ILogger<FileComicOutput> _logger;

        public FileComicOutput(ILogger<FileComicOutput> logger) 
        {
            _logger = logger;
        }

        public void SaveCategories(List<Category> categories, string fileName)
        {
            try
            {
                _logger.LogInformation("Save data categories to .csv file.");
                var writer = new StreamWriter(fileName ?? $"categories_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.csv");
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(categories);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            
        }

        public void SaveComics(List<Comic> comics, string fileName)
        {
            try
            {
                _logger.LogInformation("Save data comics to .csv file.");
                var writer = new StreamWriter(fileName ?? $"comics_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.csv");
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(comics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
        }

        public void SaveChapters(List<Chapter> chapters, string fileName)
        {
            try
            {
                _logger.LogInformation("Save data chapters of comic to .epub file.");
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
                writer.Write(fileName ?? $"comic_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.epub");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            
        }
    }
}
