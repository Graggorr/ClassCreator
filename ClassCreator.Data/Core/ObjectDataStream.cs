using ClassCreator.Data.Utility.Entity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace ClassCreator.Data.Core
{
    /// <summary>
    /// Class which handles file stuff (creating, deletion, reading, writing)
    /// </summary>
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
        
        /// <summary>
        /// Creates files with .json and .cs extensions (if they are not exist) and writes data into them
        /// </summary>
        /// <param name="typeName">The name of file</param>
        /// <param name="jsonData">Data for .json file</param>
        /// <param name="csharpData">Data for .cs file</param>
        /// <returns>True if operation has been successed; otherwise - false</returns>
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

        /// <summary>
        /// Removes .json and .cs files with set file name
        /// </summary>
        /// <param name="typeName">File name</param>
        /// <param name="cancellationToken">An instance of <see cref="CancellationToken"/> for cancelling operation after some time passing</param>
        /// <returns>True if files have been removed successfuly; otherwise - false</returns>
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

        /// <summary>
        /// Returns an instance of <see cref="ObjectData"/> which data is contained in the set path
        /// </summary>
        /// <param name="path">The path of file</param>
        /// <returns>An instance of <see cref="ObjectData"/></returns>
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

        /// <summary>
        /// Returns the full path of the file with chosen file name and extension
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="isJsonFile">Determines if need to return .json extension or .cs</param>
        /// <returns>A new built <see cref="String"/></returns>
        public static string GetFullPath(string fileName, bool isJsonFile = true) =>
            Path.Combine(_classDirectoryPath, fileName + (isJsonFile ? JSON_EXTENSION : CSHARP_EXTESNION));

        /// <summary>
        /// Returns paths of all contained objects with .json extensions
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerable{T}"/> which contains all paths</returns>
        public static IEnumerable<string> GetAllPaths() => Directory.GetFiles(_classDirectoryPath).Where(x => x.Contains(JSON_EXTENSION));

        /// <summary>
        /// Returns <see cref="Boolean"/> value of result of file existing verification
        /// </summary>
        /// <param name="typeName">Name of file</param>
        /// <returns>True if both file exist; otherwise - false</returns>
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
