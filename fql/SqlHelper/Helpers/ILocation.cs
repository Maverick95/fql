using System.Reflection;

namespace SqlHelper.Helpers
{
    public interface ILocation
    {
        public string Location();
    }

    public class AppResourceConfigLocation: ILocation
    {
        private const string _dataSubPath = "data";

        public string Location()
        {
            var exeAsmPath = Assembly.GetExecutingAssembly().Location;
            var exeAsmDirectory = Path.GetDirectoryName(exeAsmPath);
            var location = Path.Combine(exeAsmDirectory, _dataSubPath);
            return location;
        }
    }
}
