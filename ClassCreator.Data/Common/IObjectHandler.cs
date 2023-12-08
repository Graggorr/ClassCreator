using ClassCreator.Data.Utility.DTO;

namespace ClassCreator.Data.Common
{
    public interface IObjectHandler
    {
        public bool Add(ObjectDataDto objectDataDto);
        public bool Update(ObjectDataDto objectDataDto);
        public ObjectDataDto? Get(string typeName);
        public IEnumerable<ObjectDataDto> GetAll();
        public bool Remove(string typeName);
        public object? TryGetInstance(string typeName);
    }
}
