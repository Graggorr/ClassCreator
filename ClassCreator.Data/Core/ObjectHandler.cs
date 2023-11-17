using ClassCreator.Data.Common;
using ClassCreator.Data.Utility;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.AsyncEx;
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

        public async Task<bool> AddOrUpdate(ObjectDataDto objectDataDto)
        {
            var methodName = nameof(AddOrUpdate);

            var objectData = _objectDataParser.CreateObjectData(objectDataDto);

            if (objectData is null)
            {
                return false;
            }

            var assembly = _assemblyHelper.GetDynamicAssembly(objectData);

            if (assembly is null)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot perform operation");

                return false;
            }

            var result = assembly.CreateInstance(objectData.Name);

            if (result is null)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot create the instance of {objectData.Name}");

                return false;
            }

            var isFileExist = false;
            var path = Path.Combine(_classDirectoryPath, objectData.Name, CSHARP_EXTESNION);

            if (File.Exists(path))
            {
                isFileExist = true;
            }

            await File.WriteAllTextAsync(path, objectData.ToString());
            var value = isFileExist ? "updated." : "added.";
            _logger.Log(LogLevel.Information, $"{methodName} - {objectData.Name} has been {value}");

            return true;
        }

        public IEnumerable<ObjectDataDto> GetAll()
        {
            var asyncCollection = new AsyncCollection<ObjectDataDto>();
            var allPaths = Directory.GetFiles(_classDirectoryPath).Where(x => x.Contains(JSON_EXTENSION));

            var tasks = allPaths.Select(path =>
            {
                return Task.Run(async () =>
                {
                    var objectData = await GetObjectDataFromFile(path);

                    if (objectData is not null)
                    {
                        var dto = ObjectDataParser.GetObjectDataDto(objectData);
                        await asyncCollection.AddAsync(dto);
                    }
                });
            }).ToArray();

            Task.WaitAll(tasks);

            return asyncCollection.GetConsumingEnumerable().ToList();
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

            var assembly = _assemblyHelper.GetDynamicAssembly(objectData);

            if (assembly is null)
            {
                _logger.Log(LogLevel.Debug, $"{methodName} - Cannot create an assembly for chosen type {objectData.Name}.");
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot create instance of chosen type.");

                return null;
            }

            return assembly.CreateInstance(objectData.Name);
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
