using SqlHelper.Models;

namespace fql.UserInterface.Choices.Formatters
{
    public class FilterChoice
    {
        public Table Table { get; set; }
        public Column Column { get; set; }
    }

    public class FilterChoiceFormatter : IChoiceFormatter<FilterChoice>
    {
        private readonly int _padding;
        
        public FilterChoiceFormatter(int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _padding = padding;
        }

        public IEnumerable<string> Format(IEnumerable<FilterChoice> choices)
        {
            if (!choices.Any())
            {
                return new List<string>();
            }

            var column_max_length =
                choices.Max(data => data.Column.Name.Length);

            var schema_max_length =
                choices.Max(data => data.Table.Schema.Length);

            var column_space = column_max_length + _padding;
            var schema_space = schema_max_length + _padding + 1; // Extra space for the . separator

            var outputs = choices.Select(choice =>
                $"{choice.Column.Name}".PadRight(column_space) +
                $"{choice.Table.Schema}.".PadRight(schema_space) +
                $"{choice.Table.Name}");

            return outputs;
        }
    }
}
