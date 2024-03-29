using Devsoft.DB.Models;
using Devsoft.DB.Models.Linq.MsSQL;
using GeographySpider.Models;
using Microsoft.Extensions.Logging;

namespace GeographySpider.Services
{
    public class SQLGeographySaveOutput : IGeographySaveOutput
    {
        private readonly ILogger<SQLGeographySaveOutput> _logger;

        public SQLGeographySaveOutput(ILogger<SQLGeographySaveOutput> logger) 
        {
            _logger = logger;
        }
        
        public int SaveProvices(List<Province> provinces)
        {
            try
            {
                _logger.LogInformation("Save data provinces to DB.");
                var paramValues = provinces.TableValue($"udtProvince");
                return DB.Default.ExecuteSp($"MergeProvinces", new { source = paramValues });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            return 0;
        }

        public int SaveDistricts(List<District> districts)
        {
            try
            {
                _logger.LogInformation("Save data districts to DB.");
                var paramValues = districts.TableValue($"udtDistrict");
                return DB.Default.ExecuteSp($"MergeDistricts", new { source = paramValues });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            return 0;
        }

        public int SaveWards(List<Ward> wards)
        {
            try
            {
                _logger.LogInformation("Save data wards to DB.");
                var paramValues = wards.TableValue($"udtWard");
                return DB.Default.ExecuteSp($"MergeWards", new { source = paramValues });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: " + ex.Message);
            }
            return 0;
        }
    }
}
