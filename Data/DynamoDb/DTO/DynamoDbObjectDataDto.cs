using ClassCreator.Data.Common;

namespace ClassCreator.Data.DynamoDb.DTO
{
    public class DynamoDbObjectDataDto : IObjectDataDto
    {
        public string Name { get; set ; }
        public List<DynamoDbPropertyDataDto> Properties { get; set; }
    }
}
