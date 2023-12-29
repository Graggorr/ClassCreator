using Newtonsoft.Json;

namespace ClassCreator.Data.DynamoDb.Entity
{
    internal class DynamoDbPropertyData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public dynamic Value { get; set; }
    }
}
