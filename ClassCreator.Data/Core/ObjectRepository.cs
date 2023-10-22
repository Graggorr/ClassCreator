using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    internal class ObjectRepository : IObjectRepository
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<ObjectData> _objectDataCollection;
        private readonly IDictionary<Type, ObjectData> _objectDataDictionary;
        private readonly Assembly _assembly;

        public ObjectRepository(ILogger logger, Assembly assembly)
        {
            _logger = logger;
            _assembly = assembly;
            _objectDataDictionary = new ConcurrentDictionary<Type, ObjectData>();
            _objectDataCollection = _objectDataDictionary.Values;
        }

        public bool Add(Type type, ObjectData objectData)
        {
            var methodName = nameof(Add);

            if (type == null || objectData == null)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot add a new object into the repository in case of null exception. Key: {type?.ToString()}; Value: {objectData?.ToString()}");

                return false;
            }

            try
            {
                _objectDataDictionary.Add(type, objectData);
                _logger.Log(LogLevel.Debug, $"{methodName} - A new object {objectData.Name} has been added into the repository");

                return true;
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, $"{methodName} - Cannot add a new object {objectData.Name} into the repository in case of ERROR: {exception.Message}");

                return false;
            }
        }

        public ObjectData? Get(Type type) => _objectDataDictionary.TryGetValue(type, out var objectData) ? objectData : null;

        public bool Remove(Type type)
        {
            var methodName = nameof(Remove);

            if (type == null)
            {
                return false;
            }

            var result = _objectDataDictionary.Remove(type);

            if (result)
            {
                _logger.Log(LogLevel.Debug, $"{methodName} - Object {type.Name} has been removed from the repository");
            }

            return result;
        }

        public IEnumerator<ObjectData> GetEnumerator() => _objectDataCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
