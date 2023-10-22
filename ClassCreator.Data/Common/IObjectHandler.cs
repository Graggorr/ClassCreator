using ClassCreator.Data.Core;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;

namespace ClassCreator.Data.Common
{
    public interface IObjectHandler
    {
        public bool AddObjectData(ObjectDataDto objectDataDto);
        public ObjectData? GetObjectData(string name);
        public IEnumerable<ObjectData> GetObjectsData();
        public bool RemoveObjectData(string name);
        public bool RemoveObjectData(Type type);
    }
}
