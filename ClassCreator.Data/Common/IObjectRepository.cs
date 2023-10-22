using ClassCreator.Data.Utility.Entity;

namespace ClassCreator.Data.Common
{
    internal interface IObjectRepository: IEnumerable<ObjectData>
    {
        public ObjectData? this[Type key] => Get(key);
        public bool Add(Type type, ObjectData objectData);
        public ObjectData? Get(Type type);
        public bool Remove(Type type);
    }
}
