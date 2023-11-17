using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using System.Reflection;
using Nito.AsyncEx;
using ClassCreator.Data.Utility;

namespace ClassCreator.Data.Core
{
    internal class ObjectDataParser
    {
        private readonly AssemblyHelper _assemblyHelper;

        public ObjectDataParser(AssemblyHelper assemblyHelper)
        {
            _assemblyHelper = assemblyHelper;
        }

        public ObjectData? CreateObjectData(ObjectDataDto dto)
        {
            if (dto is null || string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.DataType) || string.IsNullOrEmpty(dto.AccessModifier))
            {
                return null;
            }

            var asyncCollection = new AsyncCollection<PropertyData>();

            var tasks = dto.PropertyData.Select(async x =>
            {
                var result = CreatePropertyData(x);

                if (result != null)
                {
                    await asyncCollection.AddAsync(result);

                    return true;
                }

                return false;
            });

            Task.WaitAll(tasks.ToArray());

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
                        Properties = asyncCollection.GetConsumingEnumerable().ToList(),
                    };
                }
            }

            return null;
        }

        public static ObjectDataDto GetObjectDataDto(ObjectData objectData)
        {
            var dto = new ObjectDataDto()
            {
                Name = objectData.Name,
            };

            var asyncCollection = new AsyncCollection<PropertyDataDto>();

            var tasks = objectData.Properties.Select(x =>
            {
                return asyncCollection.AddAsync(new PropertyDataDto
                {
                    Name = x.Name,
                    PropertyType = x.PropertyType.Name,
                    AccessModifier = x.AccessModifier.ToString(),
                    GetterAccessModifier = x.GetterAccessModifier.ToString() ?? string.Empty,
                    SetterAccessModifier = x.SetterAccessModifier.ToString() ?? string.Empty,
                });
            });

            Task.WaitAll(tasks.ToArray());

            dto.PropertyData = asyncCollection.GetConsumingEnumerable().ToList();

            return dto;
        }

        private PropertyData? CreatePropertyData(PropertyDataDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.SetterAccessModifier))
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

                return setterModifier is not null;
            });
            var getterModifierTask = Task.Run(() =>
            {
                getterModifier = AccessModifierToEnum(dto.GetterAccessModifier);

                return getterModifier is not null;
            });

            var tasks = new Task<bool>[] { nameTask, typeTask, accessModifierTask, getterModifierTask, setterModifierTask };
            Task.WaitAll(tasks);

            if (!tasks.All(x => x.Result) || !((int)accessModifier.Value >= (int)getterModifier.Value && (int)accessModifier.Value >= (int)setterModifier.Value))
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
                _ => null,
            };
        }

        private Type? GetType(string typeName)
        {
            Type? type = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var cancellationTokenSource = new CancellationTokenSource();

            var tasks = assemblies.Select(x =>
            {
                return Task.Run(() =>
                {
                    var result = x.GetTypes().FirstOrDefault(y => y.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));

                    if (result is not null)
                    {
                        type = result;
                        cancellationTokenSource.Cancel();
                    }
                }, cancellationTokenSource.Token);
            }).ToArray();

            Task.WaitAll(tasks);

            if (type is null)
            {
                var path = ObjectHandler.GetFullPath(typeName);
                var objectData = ObjectHandler.GetObjectDataFromFile(path).GetAwaiter().GetResult();

                if (objectData is not null)
                {
                    var assembly = _assemblyHelper.GetDynamicAssembly(objectData);
                    type = assembly?.GetTypes().FirstOrDefault(x => x.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            return type;
        }
    }
}