using ClassCreator.Data.Common;
using ClassCreator.Data.DynamoDb.DTO;

namespace ClassCreator.Data.DynamoDb.Core
{
    public class ObjectDynamoDbHandler : IObjectHandler<DynamoDbObjectDataDto>
    {
        public bool Add(DynamoDbObjectDataDto objectDataDto)
        {
            throw new NotImplementedException();
        }

        public DynamoDbObjectDataDto? Get(string typeName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DynamoDbObjectDataDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string typeName)
        {
            throw new NotImplementedException();
        }

        public bool Update(DynamoDbObjectDataDto objectDataDto)
        {
            throw new NotImplementedException();
        }
    }
}
