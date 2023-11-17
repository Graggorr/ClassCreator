using Newtonsoft.Json;

namespace ClassCreator.Data.Utility.DTO
{
    public class ObjectDataDto
    {
        [JsonProperty("accessModifier")]
        public string AccessModifier { get; internal init; }
        [JsonProperty("dataType")]
        public string DataType { get; internal init; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("propertyData")]
        public List<PropertyDataDto> PropertyData { get; set; }
    }
}
