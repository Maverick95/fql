using System.Text.Json.Serialization;

namespace SqlHelper.Models
{
    public class Column
    {
        [JsonPropertyName("table_id")]
        public long TableId { get; set; }

        [JsonPropertyName("column_id")]
        public long ColumnId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("nullable")]
        public bool Nullable { get; set; }
    }
}
