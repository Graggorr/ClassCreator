using ClassCreator.Data.Common;
using ClassCreator.Data.Files.DTO;
using ClassCreator.Data.Files.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ClassCreator.Data.Files.Core
{
    public sealed class ObjectFileHandler : IObjectHandler<FileObjectDataDto>
    {
        private readonly ILogger _logger;
        private readonly ObjectFileParser _objectDataParser;
        private readonly AssemblyHelper _assemblyHelper;
        private readonly IObjectContainer<FileObjectData> _objectContainer;

        public ObjectFileHandler(ILogger<IObjectHandler<FileObjectDataDto>> logger)
        {
            _logger = logger;
            _assemblyHelper = new AssemblyHelper(logger);
            _objectContainer = new ObjectFileContainer(logger);
            _objectDataParser = new ObjectFileParser(_objectContainer, _assemblyHelper);
        }

        public bool Add(FileObjectDataDto objectDataDto) => AddOrUpdateCore(objectDataDto, true);

        public bool Update(FileObjectDataDto objectDataDto) => AddOrUpdateCore(objectDataDto, false);

        public IEnumerable<FileObjectDataDto> GetAll()
        {
            var result = _objectContainer.GetAll();
            var convertedObjects = new ConcurrentBag<FileObjectDataDto>();

            var tasks = result.Select(objectData =>
            {
                return Task.Run(() =>
                {
                    var dto = _objectDataParser.GetObjectDataDto(objectData);
                    convertedObjects.Add(dto);
                });
            }).ToArray();

            Task.WaitAll(tasks);

            return convertedObjects;
        }

        public FileObjectDataDto? Get(string typeName)
        {
            var methodName = nameof(Get);

            if (string.IsNullOrEmpty(typeName))
            {
                _logger.Log(LogLevel.Information, $"{methodName} - Type name is empty");

                return null;
            }

            var result = _objectContainer.Get(typeName);

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
            var result = _objectContainer.Get(typeName);

            if (result is null)
            {
                return false;
            }

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

            return _objectContainer.Remove(result, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Tries to get an instance as an <see cref="object"/> of type that is contained
        /// </summary>
        /// <param name="typeName">Name of type that is contained</param>
        /// <returns>An instance as an <see cref="object"/> of chosen type if it's contained; otherwise - NULL</returns>
        public object? TryGetInstance(string typeName)
        {
            var methodName = nameof(TryGetInstance);
            var objectData = _objectContainer.Get(typeName);

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

        private bool AddOrUpdateCore(FileObjectDataDto objectDataDto, bool isToAdd)
        {
            var methodName = nameof(AddOrUpdateCore);
            var objectData = CreateObjectDataInternal(objectDataDto);

            if (objectData is null)
            {
                return false;
            }

            if (isToAdd)
            {
                if (_objectContainer.Contains(objectData))
                {
                    _logger.Log(LogLevel.Warning, $"{methodName} - The chosen object already exists.");

                    return false;
                }
            }
            else
            {
                if (!_objectContainer.Contains(objectData))
                {
                    _logger.Log(LogLevel.Warning, $"{methodName} - The chosen object does not exist.");

                    return false;
                }
            }

            var serializedData = JsonConvert.SerializeObject(objectData);
            var result = _objectContainer.SaveOrUpdate(objectData);

            if (result)
            {
                var logResult = isToAdd ? "added" : "updated";
                _logger.Log(LogLevel.Information, $"{methodName} - {objectData.Name} has been {logResult}");
            }

            return result;
        }

        private FileObjectData? CreateObjectDataInternal(FileObjectDataDto objectDataDto)
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
