using ClassCreator.Data.Common;
using ClassCreator.Data.Utility;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks.Dataflow;

namespace ClassCreator.Data.Core
{
    public class ObjectCreator : IObjectCreator
    {
        private readonly Assembly _assemblyToInputCreatedClass;

        public ObjectCreator()
        {

        }

        public bool CreateObject(ObjectData objectData)
        {

            return true;
        }

        public IEnumerable<ObjectData> GetObjects()
        {
            var types = _assemblyToInputCreatedClass.GetTypes();
        }

        private ObjectData CreateObjectData(Type type)
        {
            var objectData = new ObjectData
            {
                Name = type.Name,
            };
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var propertyDatas = new List<PropertyData>();

            foreach (var property in properties)
            {
                var propertyData = new PropertyData
                {
                    Name = property.Name,
                    Type = property.PropertyType.ToString(),
                    AccessModifier = property.
                }
            }

            return objectData;
        }
    }
}
