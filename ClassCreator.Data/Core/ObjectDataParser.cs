using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using ClassCreator.Data.Utility;
using System.Reflection;
using System.Collections.Concurrent;

namespace ClassCreator.Data.Core
{
    /// <summary>
    /// A class which parses <see cref="ObjectDataDto"/> to <see cref="ObjectData"/> and opposite.
    /// </summary>
    internal class ObjectDataParser
    {
        private readonly AssemblyHelper _assemblyHelper;

        public ObjectDataParser(AssemblyHelper assemblyHelper)
        {
            _assemblyHelper = assemblyHelper;
        }

        /// <summary>
        /// Converts incoming instance of <see cref="ObjectDataDto"/> to <see cref="ObjectData"/>
        /// </summary>
        /// <param name="dto">The chosen dto to be converted</param>
        /// <returns>A new created instance of <see cref="ObjectData"/> if validation is success; otherwise - null</returns>
        public ObjectData? CreateObjectData(ObjectDataDto dto)
        {
            if (dto is null || string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.DataType) || string.IsNullOrEmpty(dto.AccessModifier))
            {
                return null;
            }

            var concurrentBag = new ConcurrentBag<PropertyData>();
            var tasks = dto.PropertyData.Select(x =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var result = CreatePropertyData(x);

                    if (result != null)
                    {
                        concurrentBag.Add(result);

                        return true;
                    }

                    return false;
                });
            }).ToArray();

            Task.WaitAll(tasks);

            if (tasks.All(x => x.Result))
            {
                var accessModifier = AccessModifierToEnum(dto.AccessModifier);

                if (accessModifier is not null)
                {
                    return new ObjectData
                    {
                        Name = dto.Name,
                        AccessModifier = accessModifier.Value,
                        DataType = dto.DataType,
                        Properties = concurrentBag.ToList(),
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Converts incoming instance of <see cref="ObjectData"/> into the <see cref="ObjectDataDto"/>
        /// </summary>
        /// <param name="objectData">Instance of <see cref="ObjectData"/> to be converted</param>
        /// <returns>A new created instance of <see cref="ObjectDataDto"/></returns>
        public ObjectDataDto GetObjectDataDto(ObjectData objectData)
        {
            var dto = new ObjectDataDto()
            {
                Name = objectData.Name,
            };

            var concurrentBag = new ConcurrentBag<PropertyDataDto>();

            var tasks = objectData.Properties.Select(x =>
            {
                return Task.Run(() => concurrentBag.Add(new PropertyDataDto
                {
                    Name = x.Name,
                    PropertyType = x.PropertyType.Name,
                    AccessModifier = x.AccessModifier.ToString(),
                    GetterAccessModifier = x.GetterAccessModifier.ToString() ?? string.Empty,
                    SetterAccessModifier = x.SetterAccessModifier.ToString() ?? string.Empty,
                }));
            }).ToArray();

            Task.WaitAll(tasks);
            dto.PropertyData = concurrentBag.ToList();

            return dto;
        }

        private PropertyData? CreatePropertyData(PropertyDataDto dto)
        {
            if (dto is null)
            {
                return null;
            }

            MethodAttributes? accessModifier = null;
            MethodAttributes? getterModifier = null;
            MethodAttributes? setterModifier = null;
            Type? type = null;

            var nameTask = Task.Run(() => !string.IsNullOrEmpty(dto.Name) && !dto.Name.Any(x => x.Equals(' ')));
            var typeTask = Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(dto.PropertyType))
                {
                    type = GetType(dto.PropertyType);
                }

                return type is not null;
            });
            var accessModifierTask = Task.Run(() =>
            {
                accessModifier = AccessModifierToEnum(dto.AccessModifier);

                return accessModifier is not null;
            });
            var setterModifierTask = Task.Run(() =>
            {
                setterModifier = AccessModifierToEnum(dto.SetterAccessModifier);
            });
            var getterModifierTask = Task.Run(() =>
            {
                getterModifier = AccessModifierToEnum(dto.GetterAccessModifier);
            });

            var tasks = new Task<bool>[] { nameTask, typeTask, accessModifierTask };
            Task.WaitAll(tasks);
            Task.WaitAll(getterModifierTask, setterModifierTask);

            if (!tasks.All(x => x.Result))
            {
                return null;
            }

            if (setterModifier is not null && setterModifier.Value > accessModifier.Value ||
                getterModifier is not null && getterModifier.Value > accessModifier.Value)
            {
                return null;
            }

            return new PropertyData
            {
                Name = dto.Name,
                PropertyType = type,
                AccessModifier = accessModifier.Value,
                GetterAccessModifier = getterModifier,
                SetterAccessModifier = setterModifier,
            };
        }

        private static MethodAttributes? AccessModifierToEnum(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value.ToLower() switch
            {
                "private" => MethodAttributes.Private,
                "protected" => MethodAttributes.Family,
                "internal" => MethodAttributes.Assembly,
                "public" => MethodAttributes.Public,
                _ => null
            };
        }

        private Type? GetType(string typeName)
        {
            Type? type = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            typeName = ConvertTypeName(typeName);

            var tasks = assemblies.Select(x =>
            {
                return Task.Run(() =>
                {
                    var result = x.GetTypes().FirstOrDefault(y => y.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));

                    if (result is not null)
                    {
                        type = result;
                    }
                });
            }).ToArray();

            Task.WaitAll(tasks);

            if (type is null)
            {
                var path = ObjectDataStream.GetFullPath(typeName);
                var objectData = ObjectDataStream.GetObjectDataFromFile(path);

                if (objectData is not null)
                {
                    type = _assemblyHelper.CreateTypeWithDynamicAssembly(objectData);
                }
            }

            return type;
        }

        private static string ConvertTypeName(string typeName) => typeName.ToLower() switch
        {
            "ushort" => nameof(UInt16),
            "short" => nameof(Int16),
            "uint" => nameof(UInt32),
            "int" => nameof(Int32),
            "ulong" => nameof(UInt32),
            "long" => nameof(Int64),
            "nint" => nameof(IntPtr),
            "nuint" => nameof(UIntPtr),
            "float" => nameof(Single),
            _ => typeName
        };
    }
}