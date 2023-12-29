using ClassCreator.Data.Common;


namespace ClassCreator.Data.DynamoDb.Entity
{
    internal class DynamoDbObjectData : IObjectDataEntity
    {
        internal DynamoDbObjectData() { }

        public string Name { get; init; }
        public List<DynamoDbPropertyData> Properties { get; set; }
    }
}
