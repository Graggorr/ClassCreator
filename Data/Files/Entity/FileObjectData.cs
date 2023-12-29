using ClassCreator.Data.Common;
using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Files.Entity
{
    internal class FileObjectData : IObjectDataEntity
    {
        internal FileObjectData() { }

        [JsonProperty("accessModifier")]
        public MethodAttributes AccessModifier { get; init; }
        [JsonProperty("dataType")]
        public string DataType { get; init; }
        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("properties")]
        public List<FilePropertyData> Properties { get; init; }

        public override string ToString() => $"{AccessModifier.ToString().ToLower()} {DataType.ToLower()} {Name}\n{{\n\t{string.Join('\n', Properties)}\n}}";
    }
}
