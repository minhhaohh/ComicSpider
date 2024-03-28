using Devsoft.DB.Models;
using Devsoft.DB.Models.Linq.MsSQL;
using GeographySpider.Models;

namespace GeographySpider.Services
{
    public class SQLGeographySaveOutput : IGeographySaveOutput
    {
        public int SaveProvices(List<Province> provinces)
        {
            var paramValues = provinces.TableValue($"udtProvince");
            return DB.Default.ExecuteSp($"MergeProvinces", new { source = paramValues });
        }

        public int SaveDistricts(List<District> districts)
        {
            var paramValues = districts.TableValue($"udtDistrict");
            return DB.Default.ExecuteSp($"MergeDistricts", new { source = paramValues });
        }

        public int SaveWards(List<Ward> wards)
        {
            var paramValues = wards.TableValue($"udtWard");
            return DB.Default.ExecuteSp($"MergeWards", new { source = paramValues });
        }
    }
}
