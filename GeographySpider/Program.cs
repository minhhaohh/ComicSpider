using Devsoft.DB.Models;
using GeographySpider.Models;
using GeographySpider.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    static IConfiguration Configuration;

    async static Task Main(string[] args)
    {
        Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

        ConfigureDatabase();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IGeographyDownloader, DevsoftGeographyDownloader>()
            .AddSingleton<IGeographySaveOutput, SQLGeographySaveOutput>()
            .AddSingleton<DownloadManager>()
            .BuildServiceProvider();

        var downloadManager = serviceProvider.GetService<DownloadManager>();

        var provinceRows = await downloadManager.SaveProvices();
        var districtRows = await downloadManager.SaveDistricts();
        var wardRows = await downloadManager.SaveWards();
    }

    static void ConfigureDatabase()
    {
        var connectionStrings = Configuration.GetSection(DatabaseConnection.ConfigSectionName).Get<List<DatabaseConnection>>();

        foreach (var connection in connectionStrings)
        {
            DB.AddDatabaseConnection(connection.Name, connection.ConnectionString, connection.ProviderName);
        }

        DBRouter.Default.ConnectionStringName = $"{connectionStrings[0].ProviderName}.{connectionStrings[0].Name}";
    }
}
        
