using ClassCreator.Data.Utility.DTO;
using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Utility.Entity
{
    public class PropertyData
    {
        internal PropertyData() { }

        [JsonProperty("name")]
        public string Name { get; internal init; }
        [JsonProperty("propertyType")]
        public Type PropertyType { get; internal init; }
        [JsonProperty("accessModifier")]
        public MethodAttributes AccessModifier { get; internal init; }
        [JsonProperty("setterAccessModifier")]
        public MethodAttributes SetterAccessModifier { get; internal init; }
        [JsonProperty("getterAccessModifier")]
        public MethodAttributes GetterAccessModifier { get; internal init; }

        public PropertyDataDto CreatePropertyDataDto()
        {
            return new PropertyDataDto
            {
                Name = Name,
                PropertyType = PropertyType.ToString(),
                AccessModifier = AccessModifier.ToString(),
                SetterAccessModifier = SetterAccessModifier.ToString(),
                GetterAccessModifier = GetterAccessModifier.ToString(),
            };
        }

        public override string ToString()
        {
            var setterModifier = AccessModifier == SetterAccessModifier ? string.Empty : $"{SetterAccessModifier} ";
            var getterModifier = AccessModifier == GetterAccessModifier ? string.Empty : $"{GetterAccessModifier} ";

            return $"{AccessModifier} {PropertyType} {Name} {{ {getterModifier}get; {setterModifier}set; }}";
        }

        internal static PropertyData? CreatePropertyData(PropertyDataDto dto)
        {
            if (ValidatePropertyDto(dto))
            {
                return new PropertyData
                {
                    Name = dto.Name,
                    PropertyType = Type.GetType(dto.PropertyType),
                    AccessModifier = AccessModifierToEnum(dto.AccessModifier),
                    SetterAccessModifier = AccessModifierToEnum(dto.SetterAccessModifier),
                    GetterAccessModifier = AccessModifierToEnum(dto.GetterAccessModifier),
                };
            }

            return null;
        }

        private static bool ValidatePropertyDto(PropertyDataDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.SetterAccessModifier) || string.IsNullOrEmpty(dto.GetterAccessModifier))
            {
                return false;
            }

            var accessModifier = MethodAttributes.PrivateScope;
            var getterModifier = MethodAttributes.PrivateScope;
            var setterModifier = MethodAttributes.PrivateScope;

            var nameTask = new Task<bool>(() =>
            {
                if (string.IsNullOrEmpty(dto.Name) || dto.Name.Any(x => x.Equals(' ')))
                {
                    return false;
                }

                return true;
            });

            var typeTask = new Task<bool>(() =>
            {
                if (string.IsNullOrEmpty(dto.PropertyType) || Type.GetType(dto.PropertyType) == null)
                {
                    return false;
                }

                return true;
            });

            var accessModifierTask = new Task<bool>(() =>
            {
                accessModifier = AccessModifierToEnum(dto.AccessModifier);

                if (accessModifier != MethodAttributes.PrivateScope)
                {
                    return true;
                }

                return false;
            });

            var setterModifierTask = new Task<bool>(() =>
            {
                setterModifier = AccessModifierToEnum(dto.SetterAccessModifier);

                if (setterModifier != MethodAttributes.PrivateScope)
                {
                    return true;
                }

                return false;
            });

            var getterModifierTask = new Task<bool>(() =>
            {
                getterModifier = AccessModifierToEnum(dto.GetterAccessModifier);

                if (getterModifier != MethodAttributes.PrivateScope)
                {
                    return true;
                }

                return false;
            });

            var tasks = new Task<bool>[] { nameTask, typeTask, accessModifierTask, getterModifierTask, setterModifierTask };
            Array.ForEach(tasks, x => x.Start());
            Task.WaitAll(tasks);

            if (tasks.All(x => x.Result))
            {
                return (int)accessModifier >= (int)getterModifier && (int)accessModifier >= (int)setterModifier;
            }

            return false;
        }
        private static MethodAttributes AccessModifierToEnum(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return MethodAttributes.PrivateScope;
            }

            return value.ToLower() switch
            {
                "private" => MethodAttributes.Private,
                "protected" => MethodAttributes.Family,
                "internal" => MethodAttributes.Assembly,
                "public" => MethodAttributes.Public,
                _ => MethodAttributes.PrivateScope,
            };
        }
    }
}
