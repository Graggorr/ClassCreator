using Newtonsoft.Json;

namespace ClassCreator.Data.DynamoDb.DTO
{
    public class DynamoDbPropertyDataDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public dynamic Value { get; set; }
    }
}
