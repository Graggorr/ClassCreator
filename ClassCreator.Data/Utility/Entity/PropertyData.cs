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
            var setterModifier = AccessModifier == SetterAccessModifier ? string.Empty : $"{SetterAccessModifier} ";
            var getterModifier = AccessModifier == GetterAccessModifier ? string.Empty : $"{GetterAccessModifier} ";

            return $"{AccessModifier} {PropertyType} {Name} {{ {getterModifier}get; {setterModifier}set; }}";
        }
    }
}
