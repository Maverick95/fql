using System.Text.Json.Serialization;

namespace SqlHelper.Models
{
    // SortedDictionary enforces uniqueness 
    public class DbData
    {
        [JsonPropertyName("tables")]
        public SortedDictionary<long, Table> Tables { get; set; }

        [JsonPropertyName("constraints")]
        public SortedDictionary<long, Constraint> Constraints { get; set; }

        [JsonPropertyName("columns")]
        public SortedDictionary<(long TableId, long ColumnId), Column> Columns { get; set; }
    }
}
