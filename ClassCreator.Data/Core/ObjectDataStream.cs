using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    internal class ObjectDataStream
    {
        private const string FILES_DIRECTORY_NAME = "Classes";
        private const string JSON_EXTENSION = ".json";
        private const string CSHARP_EXTESNION = ".cs";

        private readonly static string _classDirectoryPath;

        private readonly ILogger _logger;

        static ObjectDataStream()
        {
            _classDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FILES_DIRECTORY_NAME);

            if (!Directory.Exists(_classDirectoryPath))
            {
                Directory.CreateDirectory(_classDirectoryPath);
            }
        }

        public ObjectDataStream(ILogger logger)
        {
            _logger = logger;
        }

        public bool WriteDataIntoFile(string typeName, string jsonData, string csharpData)
        {
            var methodName = nameof(WriteDataIntoFile);
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

        public bool RemoveFiles(string typeName, CancellationToken cancellationToken)
        {
            var methodName = nameof(RemoveFiles);
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

        public static ObjectData? GetObjectDataFromFile(string path)
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

        public static string GetFullPath(string fileName, bool isJsonFile = true) =>
            Path.Combine(_classDirectoryPath, fileName + (isJsonFile ? JSON_EXTENSION : CSHARP_EXTESNION));

        public static IEnumerable<string> GetAllPaths() => Directory.GetFiles(_classDirectoryPath).Where(x => x.Contains(JSON_EXTENSION));

        public static bool IsFilesExist(string typeName)
        {
            var jsonPath = GetFullPath(typeName, true);
            var csharpPath = GetFullPath(typeName, false);

            return File.Exists(jsonPath) || File.Exists(csharpPath);
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
