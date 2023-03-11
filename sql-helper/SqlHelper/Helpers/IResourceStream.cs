namespace SqlHelper.Helpers
{
    public interface IResourceStream
    {
        /*
         * Similar to GET
         * Returns (false, null) if file not found
         */
        public (bool, string) Read(string resource);

        /*
         * Similar to PUT
         */
        public void Write(string resource, string content);
    }

    public class AppFileResourceStream: IResourceStream
    {
        private const string _appFileDirectory = "data";

        public (bool, string) Read(string resource)
        {
            var path = Path.Combine(_appFileDirectory, resource);
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return (true, content);
            }
            return (false, null);
        }

        public void Write(string resource, string content)
        {
            Directory.CreateDirectory(_appFileDirectory);
            var path = Path.Combine(_appFileDirectory, resource);
            File.WriteAllText(path, content);
        }
    }
}
