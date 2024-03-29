using ComicSpider.Commands;
using ComicSpider.Injection;
using ComicSpider.Services;
using Microsoft.Extensions.DependencyInjection;
using NLog.Extensions.Logging;
using Spectre.Console.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var app = new CommandApp(RegisterServices());
        app.Configure(config => ConfigureCommands(config));

        app.Run(args);
    }

    private static ITypeRegistrar RegisterServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IComicDowloader, BNSComicDownloader>();
        services.AddSingleton<IComicOutput, FileComicOutput>();
        services.AddSingleton<DownloadManager>();
        services.AddSingleton<GetCategoriesCommand.Settings>();
        services.AddSingleton<GetComicsCommand.Settings>();
        services.AddSingleton<DownloadComicCommand.Settings>();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddNLog();
        });

        return new TypeRegistrar(services);
    }

    private static IConfigurator ConfigureCommands(IConfigurator config)
    {
        config.AddCommand<GetCategoriesCommand>("categories")
            .WithDescription("Get list of categories from url")
            .WithExample(new[] { "categories", "https://bachngocsach.vip/", "--file", "categories.csv" });

        config.AddCommand<GetComicsCommand>("comics")
            .WithDescription("Get list of comics from url")
            .WithExample(new[] { "comics", "https://bachngocsach.vip/the-loai/dien-van/28.html", "--page", "1", "--count", "3", "--file", "comics.csv" });

        config.AddCommand<DownloadComicCommand>("download")
            .WithDescription("Download full chapter of comic from url")
            .WithExample(new[] { "download", "https://bachngocsach.vip/truyen/ke-hoach-tu-cuu-cua-nu-phu-o-tu-la-trang/969.html", "--file", "chapters.epub", "--username", "yourusername", "--password", "yourpassword" });

        return config;
    }
}