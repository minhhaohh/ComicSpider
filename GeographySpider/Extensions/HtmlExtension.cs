using HtmlAgilityPack;

namespace GeographySpider.Extensions
{
    public static class HtmlExtension
    {
        public static HtmlNode ToDOM(this string body)
        {
            var html = new HtmlDocument();
            html.LoadHtml(body);
            return html.DocumentNode;
        }
    }
}
