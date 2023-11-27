using ClassCreator.Data.Common;
using ClassCreator.Data.Utility;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    public sealed class ObjectHandler : IObjectHandler
    {
        private const string FILES_DIRECTORY_NAME = "Classes";
        private const string JSON_EXTENSION = ".json";
        private const string CSHARP_EXTESNION = ".cs";

        private readonly static string _classDirectoryPath;

        private readonly ILogger _logger;
        private readonly ObjectDataParser _objectDataParser;
        private readonly AssemblyHelper _assemblyHelper;

        static ObjectHandler()
        {
            _classDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FILES_DIRECTORY_NAME);

            if (!Directory.Exists(_classDirectoryPath))
            {
                Directory.CreateDirectory(_classDirectoryPath);
            }
        }

        public ObjectHandler(ILogger<IObjectHandler> logger)
        {
            _logger = logger;
            _assemblyHelper = new AssemblyHelper(logger);
            _objectDataParser = new ObjectDataParser(_assemblyHelper);
        }

        internal static string ClassDirectoryPath => _classDirectoryPath;

        public async Task<bool> Add(ObjectDataDto objectDataDto) => await AddOrUpdateCore(objectDataDto, true);

        public async Task<bool> Update(ObjectDataDto objectDataDto) => await AddOrUpdateCore(objectDataDto, false);

        public IEnumerable<ObjectDataDto> GetAll()
        {
            var concurrentBag = new ConcurrentBag<ObjectDataDto>();
            var allPaths = Directory.GetFiles(_classDirectoryPath).Where(x => x.Contains(JSON_EXTENSION));

            var tasks = allPaths.Select(path =>
            {
                return Task.Run(async () =>
                {
                    var objectData = await GetObjectDataFromFile(path);

                    if (objectData is not null)
                    {
                        var dto = ObjectDataParser.GetObjectDataDto(objectData);
                        concurrentBag.Add(dto);
                    }
                });
            }).ToArray();

            Task.WaitAll(tasks);

            return concurrentBag;
        }

        public async Task<ObjectDataDto?> Get(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            var path = GetFullPath(typeName);
            var result = await GetObjectDataFromFile(path);

            if (result is null)
            {
                return null;
            }

            return ObjectDataParser.GetObjectDataDto(result);
        }

        public bool Remove(string typeName)
        {
            var methodName = nameof(Remove);

            if (string.IsNullOrEmpty(typeName))
            {
                _logger.Log(LogLevel.Information, $"{methodName} - Type name is empty");

                return false;
            }

            var csharpFilePath = GetFullPath(typeName, false);
            var jsonFilePath = GetFullPath(typeName);

            var isJsonFileReady = false;
            var isCsharpFileReady = false;

            while (!isJsonFileReady && !isCsharpFileReady)
            {
                try
                {
                    if (!isJsonFileReady)
                    {
                        using var jsonFileStream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        File.Delete(jsonFilePath);
                        isJsonFileReady = true;
                        jsonFileStream.Close();
                    }

                    if (!isCsharpFileReady)
                    {
                        using var csharpFileStream = new FileStream(csharpFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        File.Delete(csharpFilePath);
                        isCsharpFileReady = true;
                        csharpFileStream.Close();
                    }
                }
                catch { }
            }

            _logger.Log(LogLevel.Information, $"{methodName} - {typeName} has been deleted successfully.");

            return true;
        }

        public async Task<object?> TryGetInstance(string typeName)
        {
            var methodName = nameof(TryGetInstance);
            var objectData = await GetObjectDataFromFile(typeName);

            if (objectData is null)
            {
                _logger.Log(LogLevel.Warning, $"{methodName} - Cannot find contained object under chosen name.");

                return null;
            }

            var type = _assemblyHelper.CreateTypeWithDynamicAssembly(objectData);

            if (type is null)
            {
                _logger.Log(LogLevel.Debug, $"{methodName} - Cannot create an assembly for chosen type {objectData.Name}.");
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot create instance of chosen type.");

                return null;
            }

            var instance = _assemblyHelper.GetInstance(type);

            if (instance is null)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot create instance of chosen type");
            }

            return instance;
        }

        private async Task<bool> AddOrUpdateCore(ObjectDataDto objectDataDto, bool isToAdd)
        {
            var methodName = nameof(AddOrUpdateCore);
            var objectData = CreateObjectDataInternal(objectDataDto);

            if (objectData is null)
            {
                return false;
            }

            var jsonPath = Path.Combine(_classDirectoryPath, objectData.Name, JSON_EXTENSION);
            var csharpPath = Path.Combine(_classDirectoryPath, objectData.Name, CSHARP_EXTESNION);

            if (isToAdd)
            {
                if (File.Exists(jsonPath) || File.Exists(csharpPath))
                {
                    _logger.Log(LogLevel.Warning, $"{methodName} - The chosen object already exists.");

                    return false;
                }
            }
            else
            {
                if (!File.Exists(jsonPath) || !File.Exists(csharpPath))
                {
                    _logger.Log(LogLevel.Warning, $"{methodName} - The chosen object does not exist.");

                    return false;
                }
            }

            var serializedData = JsonConvert.SerializeObject(objectData);
            await File.WriteAllTextAsync(csharpPath, objectData.ToString());
            await File.WriteAllTextAsync(jsonPath, serializedData);
            var logResult = isToAdd ? "added" : "updated";
            _logger.Log(LogLevel.Information, $"{methodName} - {objectData.Name} has been {logResult}");

            return true;
        }

        private ObjectData? CreateObjectDataInternal(ObjectDataDto objectDataDto)
        {
            var methodName = nameof(CreateObjectDataInternal);
            var objectData = _objectDataParser.CreateObjectData(objectDataDto);

            if (objectData is null)
            {
                return null;
            }

            var type = _assemblyHelper.CreateTypeWithDynamicAssembly(objectData);

            if (type is null)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot perform operation");

                return null;
            }

            return objectData;
        }

        internal static async Task<ObjectData?> GetObjectDataFromFile(string path)
        {
            if (!Path.Exists(path))
            {
                return null;
            }

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            var data = await streamReader.ReadToEndAsync();
            streamReader.Close();

            return JsonConvert.DeserializeObject<ObjectData>(data);
        }

        internal static string GetFullPath(string fileName, bool isJsonFile = true) =>
            Path.Combine(_classDirectoryPath, fileName, isJsonFile ? JSON_EXTENSION : CSHARP_EXTESNION);
    }
}
