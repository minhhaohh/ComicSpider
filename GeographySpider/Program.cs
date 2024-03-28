using Devsoft.DB.Models;
using GeographySpider.Models;
using GeographySpider.Services;
using Microsoft.Extensions.Configuration;

internal class Program
{
    static IConfiguration Configuration;

    async static Task Main(string[] args)
    {
        Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

        ConfigureDatabase();

        var geographyDownloader = new DevsoftGeographyDownloader();
        var geographySaveOutput = new SQLGeographySaveOutput();

        await geographyDownloader.Initialize();

        var provinces = await geographyDownloader.GetDataProvincesAsync();
        geographySaveOutput.SaveProvices(provinces);

        var districts = await geographyDownloader.GetDataDistrictsAsync();
        geographySaveOutput.SaveDistricts(districts);

        var wards = await geographyDownloader.GetDataWardsAsync();
        geographySaveOutput.SaveWards(wards);
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
        
