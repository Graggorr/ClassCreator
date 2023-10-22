using ClassCreator.Data.Utility.DTO;
using Newtonsoft.Json;

namespace ClassCreator.Data.Utility.Entity
{
    public class ObjectData
    {
        internal ObjectData() { }

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

        public static ObjectData? CreateObjectData(ObjectDataDto dto)
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
    }
}
