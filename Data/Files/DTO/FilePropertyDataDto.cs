using Newtonsoft.Json;

namespace ClassCreator.Data.Files.DTO
{
    public class FilePropertyDataDto
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
