using ClassCreator.Data.Common;
using ClassCreator.Data.Utility;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ClassCreator.Data.Core
{
    public sealed class ObjectHandler : IObjectHandler
    {
        private readonly ILogger _logger;
        private readonly ObjectDataParser _objectDataParser;
        private readonly AssemblyHelper _assemblyHelper;
        private readonly ObjectDataStream _objectDataStream;

        public ObjectHandler(ILogger<IObjectHandler> logger)
        {
            _logger = logger;
            _assemblyHelper = new AssemblyHelper(logger);
            _objectDataStream = new ObjectDataStream(logger);
            _objectDataParser = new ObjectDataParser(_assemblyHelper);
        }

        public bool Add(ObjectDataDto objectDataDto) => AddOrUpdateCore(objectDataDto, true);

        public bool Update(ObjectDataDto objectDataDto) => AddOrUpdateCore(objectDataDto, false);

        public IEnumerable<ObjectDataDto> GetAll()
        {
            var concurrentBag = new ConcurrentBag<ObjectDataDto>();
            var allPaths = ObjectDataStream.GetAllPaths();

            var tasks = allPaths.Select(path =>
            {
                return Task.Run(() =>
                {
                    var objectData = ObjectDataStream.GetObjectDataFromFile(path);

                    if (objectData is not null)
                    {
                        var dto = _objectDataParser.GetObjectDataDto(objectData);
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

            var path = ObjectDataStream.GetFullPath(typeName);
            var result = ObjectDataStream.GetObjectDataFromFile(path);

            if (result is null)
            {
                _logger.Log(LogLevel.Information, $"{methodName} - {typeName} is not found");

                return null;
            }

            return _objectDataParser.GetObjectDataDto(result);
        }

        public bool Remove(string typeName)
        {
            var methodName = nameof(Remove);

            if (Get(typeName) is null)
            {
                return false;
            }

            return _objectDataStream.RemoveFiles(typeName, new CancellationTokenSource().Token);
        }

        public object? TryGetInstance(string typeName)
        {
            var methodName = nameof(TryGetInstance);
            var objectData = ObjectDataStream.GetObjectDataFromFile(ObjectDataStream.GetFullPath(typeName));

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
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot create instance of chosen type.");
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

            if (isToAdd)
            {
                if (ObjectDataStream.IsFilesExist(objectData.Name))
                {
                    _logger.Log(LogLevel.Warning, $"{methodName} - The chosen object already exists.");

                    return false;
                }
            }
            else
            {
                if (!ObjectDataStream.IsFilesExist(objectData.Name))
                {
                    _logger.Log(LogLevel.Warning, $"{methodName} - The chosen object does not exist.");

                    return false;
                }
            }

            var serializedData = JsonConvert.SerializeObject(objectData);
            var result = _objectDataStream.WriteDataIntoFile(objectData.Name, serializedData, objectData.ToString());

            if (result)
            {
                var logResult = isToAdd ? "added" : "updated";
                _logger.Log(LogLevel.Information, $"{methodName} - {objectData.Name} has been {logResult}");
            }

            return result;
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
    }
}
