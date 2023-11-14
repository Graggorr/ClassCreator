using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;

namespace ClassCreator.Data.Common
{
    public interface IObjectHandler
    {
        public bool AddOrUpdate(ObjectDataDto objectDataDto);
        public ObjectData? Get(string name);
        public IEnumerable<ObjectData> GetAll();
        public bool Remove(string name);
        public bool Remove(Type type);
    }
}
