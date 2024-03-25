using ComicSpider.Commands;
using Spectre.Console.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<GetCategories>("categories")
                    .WithDescription("Get list of categories from url")
                    .WithExample(new[] { "categories", "https://bachngocsach.vip/" });
            config.AddCommand<GetComics>("comics")
                    .WithDescription("Get list of comics from url")
                    .WithExample(new[] { "comics", "https://bachngocsach.vip/the-loai/dien-van/28.html", "--page", "1", "--count", "3" });
            config.AddCommand<DownloadComic>("download")
                    .WithDescription("Download comic from url")
                    .WithExample(new[] { "download", "https://bachngocsach.vip/truyen/ke-hoach-tu-cuu-cua-nu-phu-o-tu-la-trang/969.html" });
        });
        app.Run(args);
    }
}