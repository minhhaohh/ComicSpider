namespace ComicSpider.Models
{
    public class Comic
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string TotalChapter { get; set; }

        public string Url { get; set; }

        public Comic() 
        {
            Title = string.Empty;
            Author = string.Empty;
            TotalChapter = string.Empty;
            Url = string.Empty;
        }

        public Comic(string title, string author, string totalChapter, string url)
        {
            Title = title;
            Author = author;
            TotalChapter = totalChapter;
            Url = url;
        }
    }
}
