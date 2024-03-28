using GeographySpider.Models;

namespace GeographySpider.Services
{
    public interface IGeographyDownloader
    {
        Task<List<Province>> GetDataProvincesAsync();

        Task<List<District>> GetDataDistrictsAsync();
        
        Task<List<Ward>> GetDataWardsAsync();
    }
}
