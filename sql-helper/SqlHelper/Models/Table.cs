using System.Text.Json.Serialization;

namespace SqlHelper.Models
{
    public class Table
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("schema")]
        public string Schema { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
