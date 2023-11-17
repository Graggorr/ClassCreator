using ClassCreator.Data.Utility.DTO;

namespace ClassCreator.Data.Common
{
    public interface IObjectHandler
    {
        public Task<bool> AddOrUpdate(ObjectDataDto objectDataDto);
        public Task<ObjectDataDto?> Get(string typeName);
        public IEnumerable<ObjectDataDto> GetAll();
        public bool Remove(string typeName);
        public Task<object?> TryGetInstance(string typeName);
    }
}
