using ClassCreator.Data.Common;
using ClassCreator.Data.Utility.DTO;
using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClassCreator.Data.Core
{
    internal class ObjectFileContainer : IObjectContainer
    {
        private const string FILES_DIRECTORY_NAME = "Classes";
        private const string JSON_EXTENSION = ".json";
        private const string CSHARP_EXTESNION = ".cs";

        private readonly static string _classDirectoryPath;

        private readonly ILogger _logger;

        static ObjectFileContainer()
        {
            _classDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FILES_DIRECTORY_NAME);

            if (!Directory.Exists(_classDirectoryPath))
            {
                Directory.CreateDirectory(_classDirectoryPath);
            }
        }
        public ObjectFileContainer(ILogger logger)
        {
            _logger = logger;
        }

        public bool SaveOrUpdate(ObjectData objectData)
        {
            var methodName = nameof(SaveOrUpdate);
            var typeName = objectData.Name;
            var jsonData = JsonConvert.SerializeObject(objectData);
            var csharpData = objectData.ToString();
            var csharpPath = GetFullPath(typeName, false);
            var jsonPath = GetFullPath(typeName);

            var tasks = new Task<bool>[2]
            {
                Task.Run(async () =>
                {
                    return await WriteDataIntoFileInternal(csharpPath, csharpData);
                }),
                Task.Run(async () =>
                {
                    return await WriteDataIntoFileInternal(jsonPath, jsonData);
                }),
            };

            Task.WaitAll(tasks);

            if (tasks.Any(x => !x.Result))
            {
                return false;
            }

            return true;
        }

        public ObjectData? Get(string typeName) => GetObjectDataFromFile(GetFullPath(typeName));

        public bool Contains(ObjectData objectData) => Get(objectData.Name) is not null;

        public IEnumerable<ObjectData> GetAll()
        {
            var concurrentBag = new ConcurrentBag<ObjectData>();
            var allPaths = GetAllPaths();

            var tasks = allPaths.Select(path =>
            {
                return Task.Run(() =>
                {
                    var objectData = GetObjectDataFromFile(path);

                    if (objectData is not null)
                    {
                        concurrentBag.Add(objectData);
                    }
                });
            }).ToArray();

            Task.WaitAll(tasks);

            return concurrentBag;
        }

        public bool Remove(ObjectData ObjectData, CancellationToken cancellationToken = default)
        {
            var methodName = nameof(Remove);
            var typeName = ObjectData.Name;
            var csharpFilePath = GetFullPath(typeName, false);
            var jsonFilePath = GetFullPath(typeName);
            var isJsonFileReady = false;
            var isCsharpFileReady = false;

            while (!isJsonFileReady || !isCsharpFileReady)
            {
                if (cancellationToken != default && cancellationToken.IsCancellationRequested)
                {
                    _logger.Log(LogLevel.Information, $"{methodName} - Cancellation is requested, {typeName}.cs and {typeName}.json has not been deleted.");

                    return false;
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

            _logger.Log(LogLevel.Information, $"{methodName} - {typeName}.cs and {typeName}.json has been deleted successfully.");

            return true;
        }

        private static string GetFullPath(string fileName, bool isJsonFile = true) =>
            Path.Combine(_classDirectoryPath, fileName + (isJsonFile ? JSON_EXTENSION : CSHARP_EXTESNION));

        private static IEnumerable<string> GetAllPaths() => Directory.GetFiles(_classDirectoryPath).Where(x => x.Contains(JSON_EXTENSION));

        private static ObjectData? GetObjectDataFromFile(string path)
        {
            if (!Path.Exists(path))
            {
                return null;
            }

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            var data = streamReader.ReadToEnd();
            streamReader.Close();

            return JsonConvert.DeserializeObject<ObjectData>(data);
        }

        private async Task<bool> WriteDataIntoFileInternal(string path, string data)
        {
            var methodName = nameof(WriteDataIntoFileInternal);

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
