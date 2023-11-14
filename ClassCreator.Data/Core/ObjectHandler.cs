using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;

namespace ClassCreator.Data.Core
{
    public sealed class ObjectHandler : IObjectHandler
    {
        private readonly AssemblyHelper _assemblyHelper;
        private readonly ILogger _logger;
        private readonly IObjectRepository _objectRepository;

        public ObjectHandler(ILogger logger)
        {
            _logger = logger;
            _assemblyHelper = new AssemblyHelper(logger);
            _objectRepository = new ObjectRepository(logger);
        }

        public bool AddOrUpdate(ObjectDataDto objectDataDto)
        {
            var methodName = nameof(AddOrUpdate);

            var objectData = ObjectData.CreateObjectData(objectDataDto);

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

            return _objectRepository.Contains(objectData.Name)
                ? _objectRepository.Update(objectData.Name, objectData)
                : _objectRepository.Add(objectData.Name, objectData);
        }

        public ObjectData? Get(string name) => string.IsNullOrEmpty(name) ? null : _objectRepository.Get(name);

        public IEnumerable<ObjectData> GetAll() => _objectRepository.ObjectsData;

        public bool Remove(string name) => !string.IsNullOrEmpty(name) && _objectRepository.Remove(name);

        public bool Remove(Type type) => Remove(type.Name);
    }
}
