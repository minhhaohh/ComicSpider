using Microsoft.Playwright;

namespace ComicSpider.Extensions
{
    public static class ElementExtension
    {
        public static async Task<string> QuerySelectorAndGetInnerTextAsync(this IPage page, string selector)
        {
            var element = await page.QuerySelectorAsync(selector);
            return element == null ? string.Empty : (await element.InnerTextAsync()).Trim();
        }

        public static async Task<string> QuerySelectorAndGetInnerTextAsync(this IElementHandle comicElement, string selector)
        {
            var element = await comicElement.QuerySelectorAsync(selector);
            return element == null ? string.Empty : (await element.InnerTextAsync()).Trim();
        }

        public static async Task<string> QuerySelectorAndGetAttributeAsync(this IElementHandle comicElement, string selector, string attribute)
        {
            var element = await comicElement.QuerySelectorAsync(selector);
            return element == null ? string.Empty : (await element.GetAttributeAsync(attribute) ?? string.Empty);
        }
    }
}
