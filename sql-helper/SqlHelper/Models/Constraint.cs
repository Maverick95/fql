using System.Text.Json.Serialization;

namespace SqlHelper.Models
{
    public class ConstraintColumnPair
    {
        [JsonPropertyName("target_column_id")]
        public long TargetColumnId { get; set; }

        [JsonPropertyName("source_column_id")]
        public long SourceColumnId { get; set; }
    }

    public class Constraint
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("target_table_id")]
        public long TargetTableId { get; set; }

        [JsonPropertyName("source_table_id")]
        public long SourceTableId { get; set; }

        // Constraints can be on more than 1 field.
        [JsonPropertyName("columns")]
        public IList<ConstraintColumnPair> Columns { get; set; }
    }
}
