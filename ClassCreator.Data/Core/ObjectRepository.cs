using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.Entity;
using ClassCreator.Infrastructure.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    internal class ObjectRepository : IObjectRepository
    {
        private static readonly string _classesDirectoryPath;
        private static readonly string _jsonsDirectoryPath;

        private readonly ILogger _logger;
        private readonly IDictionary<string, ObjectData> _objectDataDictionary;

        static ObjectRepository()
        {
            _classesDirectoryPath = Path.Combine(Path.GetFullPath(Assembly.GetExecutingAssembly().Location), "Repository", "Classes");
            _jsonsDirectoryPath = Path.Combine(Path.GetFullPath(Assembly.GetExecutingAssembly().Location), "Repository", "Classes");
            Directory.CreateDirectory(_classesDirectoryPath);
            Directory.CreateDirectory(_jsonsDirectoryPath);
        }

        public ObjectRepository(ILogger logger)
        {
            _logger = logger;
            _objectDataDictionary = new ConcurrentDictionary<string, ObjectData>();
            InitializeDictionary();
            ObjectsData = _objectDataDictionary.Values;
        }

        public IEnumerable<ObjectData> ObjectsData { get; private init; }

        public bool Add(string name, ObjectData objectData)
        {
            var methodName = nameof(Add);

            var dictionaryTask = Task.Run(() =>
            {
                _objectDataDictionary.Add(name, objectData);
                _logger.Log(LogLevel.Debug, $"{methodName} - A new object {objectData.Name} has been added into the repository.");
            });

            var fileTask = Task.Factory.StartNew(() => WriteDataIntoFiles(objectData));
            Task.WaitAll(dictionaryTask, fileTask);

            if (!fileTask.Result)
            {
                _objectDataDictionary.Remove(name);
            }

            return fileTask.Result;
        }

        public bool Update(string name, ObjectData objectData)
        {
            var methodName = nameof(Update);
            var oldObjectData = Get(name);

            if (oldObjectData is null)
            {
                _logger.Log(LogLevel.Warning, $"{methodName} - {objectData.Name} is not contained into the repository.");

                return false;
            }

            _objectDataDictionary[name] = objectData;
            var result = WriteDataIntoFiles(objectData);

            if (result)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {objectData.Name} has been updated.");
            }
            else
            {
                _objectDataDictionary[name] = oldObjectData;
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot update {objectData.Name}");
            }

            return result;
        }

        public bool Contains(string name) => _objectDataDictionary.ContainsKey(name) && Path.Exists(GetClassPath(name));
        public ObjectData? Get(string name) => Contains(name) ? _objectDataDictionary[name] : null;

        public bool Remove(string name)
        {
            var methodName = nameof(Remove);

            if (_objectDataDictionary.TryGetValue(name, out var objectData))
            {
                _objectDataDictionary.Remove(name);
                _logger.Log(LogLevel.Information, $"{methodName} - Object {name} has been removed from the repository");
                File.Delete(GetClassPath(name));
                _logger.Log(LogLevel.Debug, $"{methodName} - Class file {name}.cs has been deleted");
                File.Delete(GetJsonPath(name));
                _logger.Log(LogLevel.Debug, $"{methodName} - Json file {name}.json has been deleted");

                return true;
            }

            _logger.Log(LogLevel.Warning, $"{methodName} - Object {name} is not contained in the repository to be removed");

            return false;
        }

        public IEnumerator<ObjectData> GetEnumerator() => ObjectsData.GetEnumerator();

        private bool WriteDataIntoFiles(ObjectData objectData)
        {
            var methodName = nameof(WriteDataIntoFiles);

            var classFileTask = Task.Factory.StartNew(() =>
            {
                var classFullPath = GetClassPath(objectData.Name);

                try
                {
                    var fullCsFileData = $"namespace {Global.OBJECTS_ASSEMBLY_NAME}\n{{\n{objectData}\n}}";
                    File.WriteAllText(classFullPath, fullCsFileData);
                    _logger.Log(LogLevel.Information, $"{methodName} - A new class {objectData.Name}.cs has been added.");

                    return true;
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, $"{methodName} - Cannot create the file with data.");

                    if (Path.Exists(classFullPath))
                    {
                        File.Delete(classFullPath);
                    }

                    return false;
                }
            });

            var jsonFileTask = Task.Factory.StartNew(() =>
            {
                var jsonFullPath = GetJsonPath(objectData.Name);

                try
                {
                    var serializedData = JsonConvert.SerializeObject(objectData);
                    File.WriteAllText(jsonFullPath, serializedData);
                    _logger.Log(LogLevel.Debug, $"{methodName} - A new {objectData.Name}.json file has been added.");

                    return true;
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, $"{methodName} - Cannot create the file with data.");

                    if (Path.Exists(jsonFullPath))
                    {
                        File.Delete(jsonFullPath);
                    }

                    return false;
                }
            });

            Task.WaitAll(classFileTask, jsonFileTask);

            return classFileTask.Result && jsonFileTask.Result;
        }

        private static string GetJsonPath(string name) => Path.Combine(_jsonsDirectoryPath, $"{name}.json");
        private static string GetClassPath(string name) => Path.Combine(_classesDirectoryPath, $"{name}.cs");

        private void InitializeDictionary()
        {
            var allFilesPath = Directory.GetFiles(_jsonsDirectoryPath);

            Array.ForEach(allFilesPath, path =>
            {
                Task.Run(() =>
                {
                    var serializedData = File.ReadAllText(path);
                    var deserializedData = JsonConvert.DeserializeObject<ObjectData>(serializedData);
                    _objectDataDictionary.Add(deserializedData.Name, deserializedData);
                });
            });
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
