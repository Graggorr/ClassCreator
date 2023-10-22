using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    public class ObjectHandler : IObjectHandler
    {
        private readonly ILogger _logger;
        private readonly IObjectRepository _objectRepository;
        private readonly Assembly _assemblyToInputCreatedClass;

        public ObjectHandler(ILogger logger)
        {
            _logger = logger;
            var assemblyName = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(x => x.Name.Equals("ClassCreator.Objects"));
            _assemblyToInputCreatedClass = Assembly.Load(assemblyName);
            _objectRepository = new ObjectRepository(logger, _assemblyToInputCreatedClass);
        }

        public bool AddObjectData(ObjectDataDto objectDataDto)
        {
            var methodName = nameof(AddObjectData);
            var objectData = ObjectData.CreateObjectData(objectDataDto);

            if (objectData == null || AddNewClassInAssembly(objectData))
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot add a new object {objectData.Name} in case of null issue.");

                return false;
            }

            var result = _assemblyToInputCreatedClass.CreateInstance(objectData.Name);

            if (result == null)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot add a new object {objectData.Name} in case of impossible instance creation.");

                if (!RemoveClassFromAssembly(objectData))
                {
                    _logger.Log(LogLevel.Critical, $"{methodName} - Cannot remove file from the assembly after failed addition!!");
                }

                return false;
            }

            if (!_objectRepository.Add(result.GetType(), objectData))
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot add a new object {objectData.Name} in case of failed addition into the repository.");

                if (!RemoveClassFromAssembly(objectData))
                {
                    _logger.Log(LogLevel.Critical, $"{methodName} - Cannot remove file from the assembly after failed addition!!");
                }

                return false;
            }

            _logger.Log(LogLevel.Information, $"{methodName} - A new object has been added.");

            return true;
        }

        public ObjectData? GetObjectData(string name)
        {
            var type = _assemblyToInputCreatedClass.GetType(name);

            if (type == null)
            {
                return null;
            }

            return _objectRepository.Get(type);
        }

        public IEnumerable<ObjectData> GetObjectsData()
        {
            var types = _assemblyToInputCreatedClass.GetTypes();
            var collection = new List<ObjectData>();
            Array.ForEach(types, t =>
            {
                var result = CreateObjectData(t);

                if (result != null)
                {
                    collection.Add(result);
                }
            });

            return collection;
        }

        public bool RemoveObjectData(string name)
        {
            var type = _assemblyToInputCreatedClass.GetType(name);

            return RemoveObjectData(type);
        }

        public bool RemoveObjectData(Type type) => _objectRepository.Remove(type);

        private bool AddNewClassInAssembly(ObjectData objectData)
        {
            var methodName = nameof(AddNewClassInAssembly);
            var objectString = objectData.ToString();
            var assemblyNamespace = _assemblyToInputCreatedClass.FullName;
            var fullCsFileData = $"namespace {assemblyNamespace}\n{{\n{objectString}\n}}";
            var fileName = $"{objectData.Name}.cs";
            var path = Path.GetDirectoryName(_assemblyToInputCreatedClass.Location);
            var fullPath = Path.Combine(path, fileName);

            if (Path.Exists(fullPath))
            {
                _logger.Log(LogLevel.Warning, $"{methodName} - Cannot add a new class in case of existing one.");

                return false;
            }

            File.WriteAllText(fullPath, fullCsFileData);
            _logger.Log(LogLevel.Information, $"{methodName} - A new class has been added.");

            return true;
        }

        private bool RemoveClassFromAssembly(ObjectData objectData)
        {
            var methodName = nameof(RemoveClassFromAssembly);
            var fileName = $"{objectData.Name}.cs";
            var path = Path.GetDirectoryName(_assemblyToInputCreatedClass.Location);
            var fullPath = Path.Combine(path, fileName);

            try
            {
                File.Delete(fullPath);
                _logger.Log(LogLevel.Information, $"{methodName} - File {fileName} has been deleted.");

                return true;
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot delete the file {fileName} in case of ERROR: {exception.Message}");

                return false;
            }
        }

        private static ObjectData CreateObjectData(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var propertyDatas = new List<PropertyData>();

            foreach (var property in properties)
            {
                var setterAccessModifier = property.SetMethod.Attributes;
                var getterAccessModifier = property.GetMethod.Attributes;
                propertyDatas.Add(new PropertyData
                {
                    Name = property.Name,
                    PropertyType = property.PropertyType.ToString(),
                    AccessModifier = (MethodAttributes)Math.Max((int)getterAccessModifier, (int)setterAccessModifier),
                    SetterAccessModifier = setterAccessModifier,
                    GetterAccessModifier = getterAccessModifier,
                });
            }

            return new ObjectData
            {
                Name = type.Name,
                Properties = propertyDatas,
            };
        }
    }
}
