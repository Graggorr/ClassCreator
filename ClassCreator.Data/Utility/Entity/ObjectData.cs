using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Utility.Entity
{
    internal class ObjectData
    {
        internal ObjectData() { }

        [JsonProperty("accessModifier")]
        public MethodAttributes AccessModifier { get; internal init; }
        [JsonProperty("dataType")]
        public string DataType { get; internal init; }
        [JsonProperty("name")]
        public string Name { get; internal init; }
        [JsonProperty("properties")]
        public List<PropertyData> Properties { get; internal init; }
        public override string ToString() => $"{AccessModifier.ToString().ToLower()} {DataType.ToLower()} {Name}\n{{\n{string.Join('\n', Properties)}\n}}";
    }
}
