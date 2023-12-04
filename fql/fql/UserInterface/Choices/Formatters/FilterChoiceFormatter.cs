using fql.UserInterface.Choices.Models;

namespace fql.UserInterface.Choices.Formatters
{
    public class FilterChoiceFormatter : IChoiceFormatter<FilterChoice>
    {
        private sealed class FilterChoiceComparer : IComparer<FilterChoice>
        {
            public int Compare(FilterChoice x, FilterChoice y)
            {
                var compareColumnNames = string.Compare(x.Column.Name, y.Column.Name, StringComparison.InvariantCultureIgnoreCase);
                if (compareColumnNames is not 0)
                {
                    return compareColumnNames;
                }
                var compareSchemaNames = string.Compare(x.Table.Schema, y.Table.Schema, StringComparison.InvariantCultureIgnoreCase);
                if (compareSchemaNames is not 0)
                {
                    return compareSchemaNames;
                }
                var compareTableNames = string.Compare(x.Table.Name, y.Table.Name, StringComparison.InvariantCultureIgnoreCase);
                return compareTableNames;
            }
        }

        private readonly int _padding;
        
        public FilterChoiceFormatter(int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _padding = padding;
        }

        public IEnumerable<(FilterChoice choice, string format)> Format(IEnumerable<FilterChoice> choices)
        {
            if (!choices.Any())
            {
                return new List<(FilterChoice, string)>();
            }

            var column_max_length =
                choices.Max(data => data.Column.Name.Length);

            var schema_max_length =
                choices.Max(data => data.Table.Schema.Length);

            var column_space = column_max_length + _padding;
            var schema_space = schema_max_length + _padding + 1; // Extra space for the . separator

            var outputs = choices
                .OrderBy(choice => choice, new FilterChoiceComparer())
                .Select(choice =>
                {
                    var format =
                        $"{choice.Column.Name}".PadRight(column_space) +
                        $"{choice.Table.Schema}.".PadRight(schema_space) +
                        $"{choice.Table.Name}";
                    return (choice, format);
                });

            return outputs;
        }
    }
}
