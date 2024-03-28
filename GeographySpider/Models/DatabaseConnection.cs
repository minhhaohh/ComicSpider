namespace GeographySpider.Models
{
    public class DatabaseConnection
    {
        public static readonly string ConfigSectionName = "DbConnectionStrings";

        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public string ProviderName { get; set; }
    }
}
