namespace ComicSpider.Models
{
    public class Category
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public Category() 
        { 
            Name = string.Empty;
            Url = string.Empty;
        }

        public Category(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
