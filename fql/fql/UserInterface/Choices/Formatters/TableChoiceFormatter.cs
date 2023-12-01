using SqlHelper.Models;

namespace fql.UserInterface.Choices.Formatters
{
    public class TableChoice
    {
        public Table Table { get; set; }
    }

    public class TableChoiceFormatter : IChoiceFormatter<TableChoice>
    {
        private readonly int _padding;

        public TableChoiceFormatter(int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _padding = padding;
        }

        public IEnumerable<string> Format(IEnumerable<TableChoice> choices)
        {
            if (!choices.Any())
            {
                return new List<string>();
            }

            var table_max_length =
                choices.Max(data => data.Table.Name.Length);

            var table_space = table_max_length + _padding;

            var outputs = choices.Select(choice =>
                $"{choice.Table.Name}".PadRight(table_space) +
                $"{choice.Table.Schema}");

            return outputs;
        }
    }
}
