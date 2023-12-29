using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Files.Entity
{
    internal class FilePropertyData
    {
        internal FilePropertyData() { }

        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("propertyType")]
        public Type PropertyType { get; init; }
        [JsonProperty("accessModifier")]
        public MethodAttributes AccessModifier { get; init; }
        [JsonProperty("setterAccessModifier")]
        public MethodAttributes? SetterAccessModifier { get; init; }
        [JsonProperty("getterAccessModifier")]
        public MethodAttributes? GetterAccessModifier { get; init; }

        public override string ToString()
        {
            var setter = string.Empty;
            var getter = string.Empty;

            if (SetterAccessModifier is not null)
            {
                var setterModifier = AccessModifier.Equals(SetterAccessModifier) ? string.Empty : $"{SetterAccessModifier.ToString().ToLower()} ";
                setter = $"{setterModifier}set; ";
            }

            if (GetterAccessModifier is not null)
            {
                var getterModifier = AccessModifier.Equals(GetterAccessModifier) ? string.Empty : $"{GetterAccessModifier.ToString().ToLower()} ";
                getter = $"{getterModifier}get; ";
            }

            return $"{AccessModifier.ToString().ToLower()} {ConvertPropertyType(PropertyType.Name)} {Name} {{ {getter}{setter}}}";
        }

        private static string ConvertPropertyType(string typeName) => typeName switch
        {
            nameof(UInt16) => "ushort",
            nameof(Int16) => "short",
            nameof(UInt32) => "uint",
            nameof(Int32) => "int",
            nameof(UInt64) => "ulong",
            nameof(Int64) => "long",
            nameof(IntPtr) => "nint",
            nameof(UIntPtr) => "nuint",
            nameof(Single) => "float",
            _ => typeName
        };
    }
}
