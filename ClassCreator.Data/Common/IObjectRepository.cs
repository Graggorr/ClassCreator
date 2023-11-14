using ClassCreator.Data.Utility.Entity;

namespace ClassCreator.Data.Common
{
    public interface IObjectRepository: IEnumerable<ObjectData>
    {
        public IEnumerable<ObjectData> ObjectsData { get; }
        public bool Add(string name, ObjectData objectData);
        public bool Update(string name, ObjectData objectData);
        public ObjectData? Get(string name);
        public bool Remove(string name);
        public bool Contains(string name);
    }
}
