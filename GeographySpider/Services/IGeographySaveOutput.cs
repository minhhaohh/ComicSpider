using GeographySpider.Models;

namespace GeographySpider.Services
{
    public interface IGeographySaveOutput
    {
        int SaveProvices(List<Province> provinces);

        int SaveDistricts(List<District> districts);

        int SaveWards(List<Ward> wards);
    }
}
