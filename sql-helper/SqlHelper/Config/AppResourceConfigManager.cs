using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SqlHelper.Config
{
    public class AppResourceConfigManager : IConfigManager
    {
        private readonly IFileManager _fileManager;

        private const string _dataSubPath = "data";

        public AppResourceConfigManager(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        private string AppDirectory
        {
            get
            {
                var exeAsmPath = Assembly.GetExecutingAssembly().Location;
                var exeAsmDirectory = Path.GetDirectoryName(exeAsmPath);
                
                return exeAsmDirectory;
            }
        }

        public IEnumerable<string> List()
        {
            var path = Path.Combine(AppDirectory, _dataSubPath);
            /* Assumption is that the \ character is used to separate directories */
            var rgxFileName = new Regex(@"^\\?([^\\]*)\.json$");

            var names = _fileManager.List(path)
                .Select(name => name.Substring(path.Length))
                .Select(name => rgxFileName.Match(name))
                .Where(match => match.Success)
                .Select(match => match.Groups[1].Value);

            return names;
        }

        public (bool, DbData) Read(string alias)
        {
            var path = Path.Combine(AppDirectory, _dataSubPath, $"{alias}.json");
            var (exists, content) = _fileManager.Read(path);
            if (exists)
            {
                var data = JsonSerializer.Deserialize<DbData>(content);
                return (true, data);
            }
            return (false, null);
        }

        public void Write(string alias, DbData data)
        {
            var path = Path.Combine(AppDirectory, _dataSubPath, $"{alias}.json");
            var content = JsonSerializer.Serialize(data);
            _fileManager.Write(path, content);
        }
    }
}
