using ClassCreator.Data.Common;
using Newtonsoft.Json;


namespace ClassCreator.Data.DynamoDb.Entity
{
    internal class DynamoDbObjectData : IObjectDataEntity
    {
        internal DynamoDbObjectData() { }

        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("properties")]
        public List<DynamoDbPropertyData> Properties { get; set; }
    }
}
