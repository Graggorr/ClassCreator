using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Utility.Entity
{
    internal class PropertyData
    {
        internal PropertyData() { }

        [JsonProperty("name")]
        public string Name { get; internal init; }
        [JsonProperty("propertyType")]
        public Type PropertyType { get; internal init; }
        [JsonProperty("accessModifier")]
        public MethodAttributes AccessModifier { get; internal init; }
        [JsonProperty("setterAccessModifier")]
        public MethodAttributes? SetterAccessModifier { get; internal init; }
        [JsonProperty("getterAccessModifier")]
        public MethodAttributes? GetterAccessModifier { get; internal init; }

        public override string ToString()
        {
            var setterModifier = AccessModifier.Equals(SetterAccessModifier) ? $"{SetterAccessModifier?.ToString().ToLower()} set; " : string.Empty;
            var getterModifier = AccessModifier.Equals(GetterAccessModifier) ? $"{GetterAccessModifier?.ToString().ToLower()} get; " : string.Empty;

            return $"{AccessModifier.ToString().ToLower()} {PropertyType} {Name} {{ {getterModifier}{setterModifier}}}";
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
        };
    }
}
