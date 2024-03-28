using Devsoft.DB.Models;
using Devsoft.DB.Models.Linq.MsSQL;
using GeographySpider.Models;

namespace GeographySpider.Services
{
    public class DownloadManager
    {
        private readonly IGeographyDownloader _geographyDownloader;
        private readonly IGeographySaveOutput _geographySaveOutput;

        public DownloadManager(IGeographyDownloader geographyDownloader, IGeographySaveOutput geographySaveOutput)
        {
            _geographyDownloader = geographyDownloader;
            _geographySaveOutput = geographySaveOutput;
        }

        public async Task<int> SaveProvices()
        {
            var provinces = await _geographyDownloader.GetDataProvincesAsync();
            return _geographySaveOutput.SaveProvices(provinces);
        }

        public async Task<int> SaveDistricts()
        {
            var districts = await _geographyDownloader.GetDataDistrictsAsync();
            return _geographySaveOutput.SaveDistricts(districts);
        }

        public async Task<int> SaveWards()
        {
            var wards = await _geographyDownloader.GetDataWardsAsync();
            return _geographySaveOutput.SaveWards(wards);
        }
    }
}
