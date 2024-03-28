using GeographySpider.Consts;

namespace GeographySpider.Models
{
    public class FilterInput
    {
        public string CallbackId { get; set; }

        public string GridName { get; set; }

        public string LevelCode { get; set; }

        public string LevelName { get; set; }

        public static readonly FilterInput Province = new FilterInput
        {
            CallbackId = GeographyConsts.ProvinceCallbackID,
            GridName = GeographyConsts.ProvinceGridName,
            LevelCode = "1",
            LevelName = "1"
        };

        public static readonly FilterInput District = new FilterInput
        {
            CallbackId = GeographyConsts.DistrictCallbackID,
            GridName = GeographyConsts.DistrictGridName,
            LevelCode = "2",
            LevelName = "2"
        };

        public static readonly FilterInput Ward = new FilterInput
        {
            CallbackId = GeographyConsts.WardCallbackID,
            GridName = GeographyConsts.WardGridName,
            LevelCode = "3",
            LevelName = "3"
        };
    }
}
