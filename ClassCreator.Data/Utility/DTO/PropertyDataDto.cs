using Newtonsoft.Json;

namespace ClassCreator.Data.Utility.DTO
{
    public class PropertyDataDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string PropertyType { get; set; }
        [JsonProperty("accessModifier")]
        public string AccessModifier { get; set; }
        [JsonProperty("setterAccessModifier")]
        public string SetterAccessModifier { get; set; }
        [JsonProperty("GetterAccessModifier")]
        public string GetterAccessModifier { get; set; }
    }
}
