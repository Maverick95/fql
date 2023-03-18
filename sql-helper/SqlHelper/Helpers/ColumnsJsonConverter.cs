using SqlHelper.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SqlHelper.Helpers
{
    public class ColumnsJsonConverter : JsonConverter<SortedDictionary<(long TableId, long ColumnId), Column>>
    {
        private readonly Regex _rgxColumnsKey;

        public ColumnsJsonConverter()
        {
            _rgxColumnsKey = new("^\\(([1-9][0-9]*),([1-9][0-9]*)\\)$");
        }

        public override SortedDictionary<(long TableId, long ColumnId), Column> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            /*
             * Code was lifted and adjusted from
             * https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0
             */

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var columns = new SortedDictionary<(long TableId, long ColumnId), Column>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return columns;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                var rgxMatchPropertyName = _rgxColumnsKey.Match(propertyName);
                
                if (rgxMatchPropertyName.Success == false)
                {
                    throw new JsonException();
                }

                var keyTableIdParsed = long.TryParse(rgxMatchPropertyName.Groups[1].Value, out var keyTableId);
                var keyColumnIdParsed = long.TryParse(rgxMatchPropertyName.Groups[2].Value, out var keyColumnId);

                if (keyTableIdParsed == false || keyColumnIdParsed == false)
                {
                    throw new JsonException();
                }
                
                var key = (TableId: keyTableId, ColumnId: keyColumnId);
                var value = JsonSerializer.Deserialize<Column>(ref reader);

                columns.Add(key, value);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, SortedDictionary<(long TableId, long ColumnId), Column> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                var columnKey = $"({kvp.Key.TableId},{kvp.Key.ColumnId})";
                writer.WritePropertyName(columnKey);
                JsonSerializer.Serialize(writer, kvp.Value);
            }

            writer.WriteEndObject();
        }
    }
}
