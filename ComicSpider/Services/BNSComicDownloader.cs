using ComicSpider.Extensions;
using ComicSpider.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace ComicSpider.Services
{
    public class BNSComicDownloader : IComicDowloader
    {
        private readonly ILogger<BNSComicDownloader> _logger;

        public string Domain => "bachngocsach.vip";

        public event EventHandler<DownloadEventArgs> ReportProgress;

        public BNSComicDownloader(ILogger<BNSComicDownloader> logger)
        {
            _logger = logger;
        }

        protected virtual void OnDownloadEvent(DownloadEventArgs e)
        {
            ReportProgress?.Invoke(this, e);
        }

        public async Task<List<Category>> GetCategoriesAsync(DownloadContext context, string url)
        {
            var result = new List<Category>();
            try
            {
                _logger.LogInformation("Get data categories.");
                var args = new DownloadEventArgs();
                args.Progress = 0;
                OnDownloadEvent(args);

                var page = context.Page;
                await page.GotoAsync(url);
                await page.WaitForSelectorAsync("div.container");

                var uri = new Uri(url);

                var categoryButton = await page.QuerySelectorAsync("a.item-link[data-target='#allCategory']");
                if (categoryButton == null)
                    return result;

                await categoryButton.ClickAsync();

                var categories = await page.QuerySelectorAllAsync("a.categories-item-modal.categories-modal[data-dismiss='modal']");
                if (categories == null)
                    return result;

                for (int i = 0; i < categories.Count; i++)
                {
                    result.Add(await ParseCategoryAsync(uri, categories[i]));

                    args.Progress = (i + 1) * 100 / categories.Count;
                    OnDownloadEvent(args);
                }

                args.Progress = 100;
                OnDownloadEvent(args);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            return result;
        }

        public async Task<List<Comic>> GetComicsAsync(DownloadContext context, string url, int pageNumber, int countNumber)
        {
            var result = new List<Comic>();
            try
            {
                _logger.LogInformation("Get data comics.");
                var args = new DownloadEventArgs();
                args.Progress = 0;
                OnDownloadEvent(args);

                var page = context.Page;
                await page.GotoAsync(url); 
                await page.WaitForSelectorAsync("div.container:visible");

                var uri = new Uri(url);

                for (int currentPage = pageNumber; currentPage < pageNumber + countNumber; currentPage++)
                {
                    var pageNavigationElement = await page.QuerySelectorAsync("ul.vue-pagination");
                    if (pageNavigationElement != null)
                    {
                        await GotoCurrentPageAsync(page, pageNavigationElement, pageNumber, countNumber, currentPage);
                    }
                    else
                    {
                        if (pageNumber != 1 || countNumber != 1)
                        {
                            throw new ArgumentException("There is 1 page total!!!!!!");
                        }
                    }

                    var comicElements = await page.QuerySelectorAllAsync("div.novel-item");
                    if (comicElements == null)
                        return result;

                    for (int i = 0; i < comicElements.Count; i++)
                    {
                        result.Add(await ParseComicAsync(uri, comicElements[i]));
                    }

                    args.Progress = (currentPage - (pageNumber - 1)) * 100 / countNumber;
                    OnDownloadEvent(args);
                }

                args.Progress = 100;
                OnDownloadEvent(args);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            return result;
        }

        public async Task<List<Chapter>> GetChaptersAsync(DownloadContext context, string url, string username, string password)
        {
            var result = new List<Chapter>();
            try
            {
                _logger.LogInformation("Get data chapters of comic.");
                var args = new DownloadEventArgs();
                args.Progress = 0;
                OnDownloadEvent(args);

                var page = context.Page;
                await page.GotoAsync(url);
                await page.WaitForSelectorAsync("div.container:visible");

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    var loginMessage = await Login(page, username, password);
                    if (!string.IsNullOrEmpty(loginMessage))
                        throw new ArgumentException(loginMessage);
                }

                var totalChapter = await GetTotalChapter(page);
                await GotoFirstChapterAsync(page);

                var currentChapter = 1;
                var chapterDetailElement = await page.WaitForSelectorAsync("div.chapter-detail");
                while (chapterDetailElement != null)
                {
                    var buyChapterElement = await chapterDetailElement.QuerySelectorAsync("div.box-buy-chapter");
                    if (buyChapterElement != null)
                        break;

                    var chapter = await ParseChapterContentAsync(chapterDetailElement);
                    if (chapter != null)
                        result.Add(chapter);

                    var nextChapterButtonElement = await chapterDetailElement.QuerySelectorAsync("div.btn-control a:not(.disabled):has(.fas.fa-angle-right)");
                    if (nextChapterButtonElement == null)
                        break;

                    await nextChapterButtonElement.ClickAsync();
                    chapterDetailElement = await page.WaitForSelectorAsync("div.chapter-detail");

                    args.Progress = (currentChapter) * 100 / totalChapter;
                    OnDownloadEvent(args);
                    currentChapter++;
                }

                args.Progress = 100;
                OnDownloadEvent(args);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            return result;
        }

        private async Task<Category> ParseCategoryAsync(Uri uri, IElementHandle categoryElement)
        {
            var name = (await categoryElement.InnerTextAsync()).Trim();
            var href = await categoryElement.GetAttributeAsync("href");
            return new Category(name, uri.Scheme + "://" + uri.Host + href);
        }

        private async Task<Comic> ParseComicAsync(Uri uri, IElementHandle comicElement)
        {
            var title = await comicElement.QuerySelectorAndGetInnerTextAsync("h4 a");

            var href = await comicElement.QuerySelectorAndGetAttributeAsync("h4 a", "href");

            string author = await comicElement.QuerySelectorAndGetInnerTextAsync("div.author-source a.author");

            var totalChapter = await comicElement.QuerySelectorAndGetInnerTextAsync("div.story-info span");

            return new Comic(title, author, totalChapter, uri.Scheme + "://" + uri.Host + href);
        }

        private async Task<Chapter> ParseChapterContentAsync(IElementHandle chapterDetailElement)
        {
            var chapter = new Chapter();

            var comicTitle = await chapterDetailElement.QuerySelectorAndGetInnerTextAsync("ul.list-info li p");

            var chapterTitle = await chapterDetailElement.QuerySelectorAndGetInnerTextAsync("h1.chapter-title");

            chapter.Title = chapterTitle;

            var chapterContentPartElements = await chapterDetailElement.QuerySelectorAllAsync(".webkit-chapter");
            if (chapterContentPartElements == null || chapterContentPartElements.Count == 0)
                return null;

            for (int i = 0; i < chapterContentPartElements.Count; i++)
            {
                var chapterContentPartElement = chapterContentPartElements[i];
                var paragrapElements = await chapterContentPartElement.QuerySelectorAllAsync("p");
                if (paragrapElements != null && paragrapElements.Count > 0)
                {
                    var paragrapContent = new Dictionary<int, string>();
                    foreach (var paragrapElement in paragrapElements)
                    {
                        await ParseChapterParagraphContent(paragrapElement, paragrapContent);
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
            return chapter;
        }

        private async Task ParseChapterParagraphContent(IElementHandle paragrapElement, Dictionary<int, string> paragrapContent)
        {
            var paragrapText = string.Empty;
            var paragrapOrder = await GetStylePropertyValueAsync<int>(paragrapElement, "order");
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

        private async Task GotoCurrentPageAsync(IPage page, IElementHandle pageNavigationElement, int pageNumber, int countNumber, int currentPage)
        {
            var pageNumberElements = await pageNavigationElement.QuerySelectorAllAsync("li.vue-page-item");
            var lastPageNumber = await pageNumberElements[pageNumberElements.Count - 1].InnerTextAsync();
            if (pageNumber + countNumber - 1 > int.Parse(lastPageNumber))
            {
                throw new ArgumentException($"There are {lastPageNumber} pages total!!!");
            }
            string pageNumberActive = await page.QuerySelectorAndGetInnerTextAsync("li.vue-page-item.active");
            if (currentPage != int.Parse(pageNumberActive))
            {
                await GoToPageAsync(page, pageNumberElements, currentPage, pageNumber);
            }
        }

        private async Task GoToPageAsync(IPage page, IReadOnlyList<IElementHandle> pageNumberElements, int currentPage, int pageNumber)
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

        private async Task<int> GetTotalChapter(IPage page)
        {
            var totalChapterStr = await page.QuerySelectorAndGetInnerTextAsync("div.info-story ul li span.text-green");
            var totalChapterNumber = int.Parse(Regex.Match(totalChapterStr, @"\d+").Value);

            return totalChapterNumber;
        }

        private async Task GotoFirstChapterAsync(IPage page)
        {
            var listChapterBarElement = await page.WaitForSelectorAsync("div#list-chapters:visible");
            if (listChapterBarElement != null)
            {
                await listChapterBarElement.ClickAsync();

                var listChaptersTableElement = await page.WaitForSelectorAsync("table#list-chapters:visible");
                var firstChapterRowElement = await page.WaitForSelectorAsync("table#list-chapters a.chapter:visible");

                if (firstChapterRowElement != null)
                    await firstChapterRowElement.ClickAsync();
            }
        }

        private async Task<T> GetStylePropertyValueAsync<T>(IElementHandle element, string property)
        {
            return await element.EvaluateAsync<T>(@"(element) => {
                                    return window.getComputedStyle(element).getPropertyValue('"+ property + @"');
                                }");
        }

        private async Task<string> Login(IPage page, string username, string password)
        {
            var navButtonElements = await page.QuerySelectorAllAsync("ul.nav-login-register li");
            if (navButtonElements == null || navButtonElements.Count != 3)
                return "Login button not found!!!";

            var navLoginButtonElement = await navButtonElements[1].QuerySelectorAsync("a");
            if (navLoginButtonElement == null)
                return "Login button not found!!!";

            await navLoginButtonElement.ClickAsync();

            var usernameInputElement = await page.QuerySelectorAsync("input#username");
            if (usernameInputElement == null)
                return "Username input not found!!!";
            await usernameInputElement.FillAsync(username);

            var passwordInputElement = await page.QuerySelectorAsync("input#password");
            if (passwordInputElement == null)
                return "Password input not found!!!";
            await passwordInputElement.FillAsync(password);

            var loginButtonElement = await page.QuerySelectorAsync("button.btn-form");
            if (loginButtonElement == null)
                return "Login button not found!!!";

            await loginButtonElement.ClickAsync();

            var formElement = await page.QuerySelectorAsync("form#form-1");
            if (formElement != null)
            {
                var validateMessage = await formElement.WaitForSelectorAsync("span.form-message span.text-danger");
                if (validateMessage != null)
                    return await validateMessage.InnerTextAsync();
            }

            await page.WaitForSelectorAsync("div.container:visible");

            return string.Empty;
        }
    }
}
