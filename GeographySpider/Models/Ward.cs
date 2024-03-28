using BLToolkit.DataAccess;
using Devsoft.DB.Models.Flat;

namespace GeographySpider.Models
{
    [TableName("Wards")]
    public class Ward : Model<Ward>
    {
        [PrimaryKey]
        public string Code { get; set; }

        public string Name { get; set; }

        public string LevelName { get; set; }

        public string DistrictCode { get; set; }

        public Ward()
        {
            Code = string.Empty;
            Name = string.Empty;
            LevelName = string.Empty;
            DistrictCode = string.Empty;
        }

        public Ward(string code, string name, string levelName, string districtCode)
        {
            Code = code;
            Name = name;
            LevelName = levelName;
            DistrictCode = districtCode;
        }
    }
}
