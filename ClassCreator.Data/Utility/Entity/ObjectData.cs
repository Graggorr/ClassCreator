using ClassCreator.Data.Utility.DTO;
using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Utility.Entity
{
    public class ObjectData
    {
        private ObjectData() { }

        [JsonProperty("name")]
        public string Name { get; internal init; }
        [JsonProperty("properties")]
        public List<PropertyData> Properties { get; internal init; }

        public ObjectDataDto GetObjectDataDto()
        {
            var dto = new ObjectDataDto()
            {
                Name = Name,
            };

            var collection = new List<PropertyDataDto>();
            Properties.ForEach(x => collection.Add(x.CreatePropertyDataDto()));
            dto.PropertyData = collection;

            return dto;
        }

        public override string ToString() => $"public class {Name}\n{{\n{string.Join('\n', Properties)}\n}}";

        internal static ObjectData? CreateObjectData(ObjectDataDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            var properties = new List<PropertyData>();
            dto.PropertyData.ForEach(x =>
            {
                var result = PropertyData.CreatePropertyData(x);

                if (result != null)
                {
                    properties.Add(result);
                }
            });

            if (properties.Count == dto.PropertyData.Count)
            {
                return new ObjectData
                {
                    Name = dto.Name,
                    Properties = properties,
                };
            }

            return null;
        }

        internal static ObjectData? CreateObjectData(string typeName)
        {
            var type = Type.GetType(typeName);

            if (type is null)
            {
                return null;
            }

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var propertiesData = new List<PropertyData>();

            foreach (var property in properties)
            {
                var setterAccessModifier = property.SetMethod.Attributes;
                var getterAccessModifier = property.GetMethod.Attributes;
                propertiesData.Add(new PropertyData
                {
                    Name = property.Name,
                    PropertyType = property.PropertyType,
                    AccessModifier = (MethodAttributes)Math.Max((int)getterAccessModifier, (int)setterAccessModifier),
                    SetterAccessModifier = setterAccessModifier,
                    GetterAccessModifier = getterAccessModifier,
                });
            }

            return new ObjectData
            {
                Name = typeName,
                Properties = propertiesData,
            };
        }
    }
}
