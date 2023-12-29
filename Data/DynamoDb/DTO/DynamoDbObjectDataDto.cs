using ClassCreator.Data.Common;
using Newtonsoft.Json;

namespace ClassCreator.Data.DynamoDb.DTO
{
    public class DynamoDbObjectDataDto : IObjectDataDto
    {
        [JsonProperty("name")]
        public string Name { get; set ; }
        [JsonProperty("properties")]
        public List<DynamoDbPropertyDataDto> Properties { get; set; }
    }
}
