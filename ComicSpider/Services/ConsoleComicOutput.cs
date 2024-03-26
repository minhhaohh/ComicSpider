using ComicSpider.Models;
using Spectre.Console;

namespace ComicSpider.Services
{
    public class ConsoleComicOutput : IComicOutput
    {
        public void SaveCategories(List<Category> categories, string fileName)
        {
            var table = new Table()
            {
                Title = new TableTitle("LIST OF CATEGORIES", "bold green"),
                Border = TableBorder.Rounded
            };
            table.AddColumns(
                "[blue bold]No[/]",
                "[blue bold]Gategory[/]",
                "[blue bold]URL[/]"
                );

            for ( int i = 0; i < categories.Count; i++ )
            {
                table.AddRow((i + 1).ToString(), categories[i].Name, categories[i].Url);
            }

            AnsiConsole.Write(table);
        }

        public void SaveComics(List<Comic> comics, string fileName)
        {
            var table = new Table()
            {
                Title = new TableTitle("LIST OF COMICS", "bold green"),
                Border = TableBorder.Rounded
            };
            table.AddColumns(
                "[blue bold]No[/]",
                "[blue bold]Title[/]",
                "[blue bold]Author[/]",
                "[blue bold]Total Chapter[/]",
                "[blue bold]URL[/]"
                );

            for ( int i = 0; i < comics.Count; i++ )
            {
                table.AddRow((i + 1).ToString(), comics[i].Title, comics[i].Author, comics[i].TotalChapter.ToString(), comics[i].Url);
            }

            AnsiConsole.Write(table);
        }

        public void SaveChapters(List<Chapter> chapters, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
