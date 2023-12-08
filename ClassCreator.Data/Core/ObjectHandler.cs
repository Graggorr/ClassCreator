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

        public bool Add(ObjectDataDto objectDataDto) => AddOrUpdateCore(objectDataDto, true);

        public bool Update(ObjectDataDto objectDataDto) => AddOrUpdateCore(objectDataDto, false);

        public IEnumerable<ObjectDataDto> GetAll()
        {
            var concurrentBag = new ConcurrentBag<ObjectDataDto>();
            var allPaths = Directory.GetFiles(_classDirectoryPath).Where(x => x.Contains(JSON_EXTENSION));

            var tasks = allPaths.Select(path =>
            {
                return Task.Run(async () =>
                {
                    var objectData = GetObjectDataFromFile(path);

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

        public ObjectDataDto? Get(string typeName)
        {
            var methodName = nameof(Get);

            if (string.IsNullOrEmpty(typeName))
            {
                _logger.Log(LogLevel.Information, $"{methodName} - Type name is empty");

                return null;
            }

            var path = GetFullPath(typeName);
            var result = GetObjectDataFromFile(path);

            if (result is null)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {typeName} is not found");

                return null;
            }

            return ObjectDataParser.GetObjectDataDto(result);
        }

        public bool Remove(string typeName)
        {
            var methodName = nameof(Remove);

            if (Get(typeName) is null)
            {
                return false;
            }

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var csharpFilePath = GetFullPath(typeName, false);
            var jsonFilePath = GetFullPath(typeName);

            var isJsonFileReady = false;
            var isCsharpFileReady = false;

            while (!isJsonFileReady || !isCsharpFileReady)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    if (!isJsonFileReady)
                    {
                        File.Delete(jsonFilePath);
                        isJsonFileReady = true;
                    }

                    if (!isCsharpFileReady)
                    {
                        File.Delete(csharpFilePath);
                        isCsharpFileReady = true;
                    }
                }
                catch { }
            }

            if (isCsharpFileReady && isJsonFileReady)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {typeName} has been deleted successfully.");

                return true;
            }

            _logger.Log(LogLevel.Error, $"{methodName} - Cannot delete {typeName}.");

            if (!isJsonFileReady)
            {
                _logger.Log(LogLevel.Debug, $"{methodName} - Cannot delete {typeName}.json file.");
            }

            if (!isCsharpFileReady)
            {
                _logger.Log(LogLevel.Debug, $"{methodName} - Cannot delete {typeName}.cs file.");
            }

            return false;
        }

        public object? TryGetInstance(string typeName)
        {
            var methodName = nameof(TryGetInstance);
            var objectData = GetObjectDataFromFile(GetFullPath(typeName));

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

        private bool AddOrUpdateCore(ObjectDataDto objectDataDto, bool isToAdd)
        {
            var methodName = nameof(AddOrUpdateCore);
            var objectData = CreateObjectDataInternal(objectDataDto);

            if (objectData is null)
            {
                return false;
            }

            var jsonPath = GetFullPath(objectData.Name, true);
            var csharpPath = GetFullPath(objectData.Name, false);

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
            var tasks = new Task<bool>[2]
            {
                Task.Run(async () =>
                {
                    return await WriteDataIntoFile(csharpPath, objectData.ToString());
                }),
                Task.Run(async () =>
                {
                    return await WriteDataIntoFile(jsonPath, serializedData);
                }),
            };

            Task.WaitAll(tasks);

            if (tasks.Any(x => !x.Result))
            {
                return false;
            }

            var logResult = isToAdd ? "added" : "updated";
            _logger.Log(LogLevel.Information, $"{methodName} - {objectData.Name} has been {logResult}");

            return true;
        }

        internal static ObjectData? GetObjectDataFromFile(string path)
        {
            if (!Path.Exists(path))
            {
                return null;
            }

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            var data = streamReader.ReadToEnd();
            streamReader.Close();
            ObjectData result = null;

            try
            {
                result = JsonConvert.DeserializeObject<ObjectData>(data);
            }
            catch { }

            return result;
        }

        internal static string GetFullPath(string fileName, bool isJsonFile = true) =>
            Path.Combine(_classDirectoryPath, fileName + (isJsonFile ? JSON_EXTENSION : CSHARP_EXTESNION));

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

        private async Task<bool> WriteDataIntoFile(string path, string data)
        {
            var methodName = nameof(WriteDataIntoFile);

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var streamWriter = new StreamWriter(fileStream);
                streamWriter.AutoFlush = true;
                await streamWriter.WriteAsync(data);
                streamWriter.Close();

                return true;
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - ERROR: {exception.Message}");

                return false;
            }
        }
    }
}
