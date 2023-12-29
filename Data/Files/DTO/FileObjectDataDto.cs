using ClassCreator.Data.Common;
using Newtonsoft.Json;

namespace ClassCreator.Data.Files.DTO
{
    public class FileObjectDataDto : IObjectDataDto
    {
        [JsonProperty("accessModifier")]
        public string AccessModifier { get; set; }
        [JsonProperty("dataType")]
        public string DataType { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("propertyData")]
        public List<FilePropertyDataDto> PropertyData { get; set; }
    }
}
