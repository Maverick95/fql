namespace SqlHelper.Helpers
{
    /*
     * Interface exists purely for mocking / testing purposes.
     * This interface will be leaky... classes that use IFileManager need to have knowledge of file management.
     */
    public interface IFileManager
    {
        /*
         * Similar to OPTIONS
         * If location not found, creates it and returns empty list.
         */
        public IEnumerable<string> List(string path);

        /*
         * Similar to GET
         * Returns (false, null) if file not found
         */
        public (bool, string) Read(string path);

        /*
         * Similar to PUT
         * It's fairly dumb, just overwrites whatever exists
         */
        public void Write(string path, string content);
    }

    public class FileManager: IFileManager
    {
        public IEnumerable<string> List(string path)
        {
            Directory.CreateDirectory(path);
            return Directory.EnumerateFiles(path);
        }

        public (bool, string) Read(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return (true, content);
            }
            return (false, null);
        }

        public void Write(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
