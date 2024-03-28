using BLToolkit.DataAccess;
using Devsoft.DB.Models.Flat;

namespace GeographySpider.Models
{
    [TableName("Provinces")]
    public class Province : Model<Province>
    {
        [PrimaryKey]
        public string Code { get; set; }

        public string Name { get; set; }

        public string LevelName { get; set; }

        public Province()
        {
            Code = string.Empty;
            Name = string.Empty;
            LevelName = string.Empty;
        }

        public Province(string code, string name, string levelName)
        {
            Code = code;
            Name = name;
            LevelName = levelName;
        }
    }
}
