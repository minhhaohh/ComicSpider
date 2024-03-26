using ComicSpider.Models;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace ComicSpider.Services
{
    public class BNSComicDownloader : IComicDowloader
    {
        public string Domain => "bachngocsach.vip";

        public event EventHandler<DownloadEventArgs> ReportProgress;

        protected virtual void OnDownloadEvent(DownloadEventArgs e)
        {
            ReportProgress?.Invoke(this, e);
        }

        public async Task<List<Category>> GetCategoriesAsync(DownloadContext context, string url)
        {
            var result = new List<Category>();

            var args = new DownloadEventArgs();
            args.progress = 0;
            OnDownloadEvent(args);

            var page = context.Page;
            await page.GotoAsync(url);
            await page.WaitForSelectorAsync("div.container");

            var uri = new Uri(url);

            var categoryButton = await page.QuerySelectorAsync("a.item-link[data-target='#allCategory']");
            if (categoryButton != null)
            {
                await categoryButton.ClickAsync();
                var categories = await page.QuerySelectorAllAsync("a.categories-item-modal.categories-modal[data-dismiss='modal']");
                for (int i = 0; i < categories.Count; i++)
                {
                    var name = (await categories[i].InnerTextAsync()).Trim();
                    var href = await categories[i].GetAttributeAsync("href");
                    result.Add(new Category(name, uri.Scheme + "://" + uri.Host + href));

                    args.progress = (i + 1) * 100 / categories.Count;
                    OnDownloadEvent(args);
                }
            }

            args.progress = 100;
            OnDownloadEvent(args);

            return result;
        }

        public async Task<List<Comic>> GetComicsAsync(DownloadContext context, string url, int pageNumber, int countNumber)
        {
            var result = new List<Comic>();

            var args = new DownloadEventArgs();
            args.progress = 0;
            OnDownloadEvent(args);

            var page = context.Page;
            await page.GotoAsync(url);
            await page.WaitForSelectorAsync("div.container");

            var uri = new Uri(url);

            for (int currentPage = pageNumber; currentPage < pageNumber + countNumber; currentPage++)
            {
                var pageNavigationElement = await page.WaitForSelectorAsync("ul.vue-pagination");
                if (pageNavigationElement != null)
                {
                    var pageNumberElements = await pageNavigationElement.QuerySelectorAllAsync("li.vue-page-item");
                    var lastPageNumber = await pageNumberElements[pageNumberElements.Count - 1].InnerTextAsync();
                    if (pageNumber + countNumber - 1 > int.Parse(lastPageNumber))
                    {
                        throw new ArgumentException($"There are {lastPageNumber} pages total!!!");
                    }
                    string pageNumberActive = await QuerySelectorAndGetInnerTextAsync(page, "li.vue-page-item.active");
                    if (currentPage != int.Parse(pageNumberActive))
                    {
                        await GoToPageAsync(page, pageNumberElements, currentPage, pageNumber);
                    }
                }
                else
                {
                    if (pageNumber != 1 || countNumber != 1)
                    {
                        throw new ArgumentException("There is 1 page total!!!!!!");
                    }
                }

                var comicElements = await page.QuerySelectorAllAsync("div.novel-item");

                if (comicElements != null)
                {

                    for (int i = 0; i < comicElements.Count; i++)
                    {
                        var comicElement = comicElements[i];

                        var title = await QuerySelectorAndGetInnerTextAsync(comicElement, "h4 a");

                        var href = await QuerySelectorAndGetAttributeAsync(comicElement, "h4 a", "href");

                        string author = await QuerySelectorAndGetInnerTextAsync(comicElement, "div.author-source a.author");

                        var totalChapter = await QuerySelectorAndGetInnerTextAsync(comicElement, "div.story-info span");

                        result.Add(new Comic(title, author, totalChapter, uri.Scheme + "://" + uri.Host + href));
                    }
                }
                args.progress = (currentPage - (pageNumber - 1)) * 100 / countNumber;
                OnDownloadEvent(args);
            }

            args.progress = 100;
            OnDownloadEvent(args);

            return result;
        }

        public async Task<List<Chapter>> GetChaptersAsync(DownloadContext context, string url)
        {
            var result = new List<Chapter>();

            var args = new DownloadEventArgs();
            args.progress = 0;
            OnDownloadEvent(args);

            var page = context.Page;
            await page.GotoAsync(url);
            await page.WaitForSelectorAsync("div.container:visible");

            var totalChapter = await GetTotalChapter(page);
            await GotoFirstChapterAsync(page);

            var currentChapter = 1;
            var chapterDetailElement = await page.WaitForSelectorAsync("div.chapter-detail");
            while (chapterDetailElement != null)
            {
                var buyChapterElement = await chapterDetailElement.QuerySelectorAsync("div.box-buy-chapter");
                if (buyChapterElement != null)
                    break;

                result.Add(await GetChapterContentAsync(chapterDetailElement));

                var nextChapterButtonElement = await chapterDetailElement.QuerySelectorAsync("div.btn-control a:not(.disabled):has(.fas.fa-angle-right)");
                if (nextChapterButtonElement == null)
                    break;

                await nextChapterButtonElement.ClickAsync();
                chapterDetailElement = await page.WaitForSelectorAsync("div.chapter-detail");

                args.progress = (currentChapter) * 100 / totalChapter;
                OnDownloadEvent(args);
                currentChapter++;
            }

            args.progress = 100;
            OnDownloadEvent(args);

            return result;
        }

        private static async Task<string> QuerySelectorAndGetInnerTextAsync(IPage page, string selector)
        {
            var element = await page.QuerySelectorAsync(selector);
            return element == null ? string.Empty : (await element.InnerTextAsync()).Trim();
        }

        private static async Task<string> QuerySelectorAndGetInnerTextAsync(IElementHandle comicElement, string selector)
        {
            var element = await comicElement.QuerySelectorAsync(selector);
            return element == null ? string.Empty : (await element.InnerTextAsync()).Trim();
        }

        private static async Task<string> QuerySelectorAndGetAttributeAsync(IElementHandle comicElement, string selector, string attribute)
        {
            var element = await comicElement.QuerySelectorAsync(selector);
            return element == null ? string.Empty : (await element.GetAttributeAsync(attribute) ?? string.Empty);
        }

        private static async Task GoToPageAsync(IPage page, IReadOnlyList<IElementHandle> pageNumberElements, int currentPage, int pageNumber)
        {
            var pageNumberChooserElement = await page.QuerySelectorAsync("li.vue-page-item.disabled.break-view-class");
            if (pageNumberChooserElement != null)
            {
                await pageNumberChooserElement.ClickAsync();

                var inputPageElement = await page.QuerySelectorAsync("div.box-option-paginate input");
                await inputPageElement.FillAsync(pageNumber.ToString());

                var buttonMoveElement = await page.QuerySelectorAsync("div.box-option-paginate button.btn.btn-primary");
                await buttonMoveElement.ClickAsync();
            }
            else
            {
                foreach (var pageNumberElement in pageNumberElements)
                {
                    var pageNum = await pageNumberElement.InnerTextAsync();
                    if (currentPage == int.Parse(pageNum))
                    {
                        await pageNumberElement.ClickAsync();
                    }
                }
            }
        }

        private static async Task<int> GetTotalChapter(IPage page)
        {
            var totalChapterStr = await QuerySelectorAndGetInnerTextAsync(page, "div.info-story ul li span.text-green");
            var totalChapterNumber = int.Parse(Regex.Match(totalChapterStr, @"\d+").Value);

            return totalChapterNumber;
        }

        private static async Task GotoFirstChapterAsync(IPage page)
        {
            var listChapterBarElement = await page.WaitForSelectorAsync("div#list-chapters:visible");
            if (listChapterBarElement != null)
            {
                await listChapterBarElement.ClickAsync();

                var listChaptersTableElement = await page.WaitForSelectorAsync("table#list-chapters:visible");
                var firstChapterRowElement = await page.WaitForSelectorAsync("table#list-chapters a.chapter:visible");

                if (firstChapterRowElement != null)
                {
                    await firstChapterRowElement.ClickAsync();
                }
            }
        }

        private static async Task<Chapter> GetChapterContentAsync(IElementHandle? chapterDetailElement)
        {
            var chapter = new Chapter();

            var comicTitle = await QuerySelectorAndGetInnerTextAsync(chapterDetailElement, "ul.list-info li p");

            var chapterTitle = await QuerySelectorAndGetInnerTextAsync(chapterDetailElement, "h1.chapter-title");

            chapter.Title = chapterTitle;

            var chapterContentPartElements = await chapterDetailElement.QuerySelectorAllAsync(".webkit-chapter");
            if (chapterContentPartElements != null && chapterContentPartElements.Count > 0)
            {
                for (int i = 0; i < chapterContentPartElements.Count; i++)
                {
                    var chapterContentPartElement = chapterContentPartElements[i];
                    var paragrapElements = await chapterContentPartElement.QuerySelectorAllAsync("p");
                    if (paragrapElements != null && paragrapElements.Count > 0)
                    {
                        var paragrapContent = new Dictionary<int, string>();
                        foreach (var paragrapElement in paragrapElements)
                        {
                            var paragrapOrder = await GetStylePropertyValueAsync<int>(paragrapElement, "order");
                            var paragrapText = string.Empty;
                            var childElements = await paragrapElement.QuerySelectorAllAsync("*");
                            if (childElements != null && childElements.Count > 0)
                            {
                                var childContent = new Dictionary<int, string>();
                                foreach (var childElement in childElements)
                                {
                                    var childOrder = await GetStylePropertyValueAsync<int>(childElement, "order");
                                    var childText = await childElement.InnerTextAsync();
                                    childContent[childOrder] = childText;
                                }
                                foreach (var childOrder in childContent.Keys.Order())
                                {
                                    paragrapText += childContent[childOrder];
                                }
                            }
                            else
                            {
                                paragrapText = await paragrapElement.InnerTextAsync();
                            }
                            paragrapContent[paragrapOrder] = paragrapText;
                        }
                        foreach (var order in paragrapContent.Keys.Order())
                        {
                            chapter.Paragraphs.Add(paragrapContent[order]);
                        }
                    }
                    else
                    {
                        var chapterContentPart = await chapterContentPartElement.InnerTextAsync();
                        chapter.Paragraphs.Add(chapterContentPart);
                    }
                }
            }
            return chapter;
        }

        private static async Task<T> GetStylePropertyValueAsync<T>(IElementHandle element, string property)
        {
            return await element.EvaluateAsync<T>(@"(element) => {
                                    return window.getComputedStyle(element).getPropertyValue('"+ property + @"');
                                }");
        }
    }
}
