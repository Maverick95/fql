using CommandLine;

namespace SqlHelper.Models
{
    public class Options
    {
        [Option('c', "connection-string")]
        public string ConnectionString { get; set; }

        [Option('a', "alias")]
        public string Alias { get; set; }

        [Option('m', "merge")]
        public bool IsMergeAliasOptionSupplied { get; set; }

        [Option('n', "new")]
        public bool IsNewAliasOptionSupplied { get; set; }

        [Option('o', "override")]
        public bool IsOverrideAliasOptionSupplied { get; set; }        
    }
}
