namespace ComicSpider.Models
{
    public class Chapter
    {
        public string Title { get; set; }

        public List<string> Paragraphs { get; set; }

        public Chapter() 
        { 
            Title = string.Empty;
            Paragraphs = new List<string>();
        }

        public Chapter(string title, List<string> paragraphs)
        {
            Title = title;
            Paragraphs = paragraphs;
        }
    }
}
