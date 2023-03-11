using CommandLine;

namespace SqlHelper.Models
{
    public class Options
    {
        [Option('c', "connection-string")]
        public string ConnectionString { get; set; }

        [Option('a', "alias")]
        public string Alias { get; set; }
    }
}
