using fql.UserInterface.Choices.Models;

namespace fql.UserInterface.Choices.Formatters
{
    public class TableChoiceFormatter : IChoiceFormatter<TableChoice>
    {
        private sealed class TableChoiceComparer : IComparer<TableChoice>
        {
            public int Compare(TableChoice x, TableChoice y)
            {
                var compareTableNames = string.Compare(x.Table.Name, y.Table.Name, StringComparison.InvariantCultureIgnoreCase);
                if (compareTableNames is not 0)
                {
                    return compareTableNames;
                }
                var compareSchemaNames = string.Compare(x.Table.Schema, y.Table.Schema, StringComparison.InvariantCultureIgnoreCase);
                return compareSchemaNames;
            }
        }

        private readonly int _padding;

        public TableChoiceFormatter(int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _padding = padding;
        }

        public IEnumerable<(TableChoice choice, string format)> Format(IEnumerable<TableChoice> choices)
        {
            if (!choices.Any())
            {
                return new List<(TableChoice, string)>();
            }

            var table_max_length =
                choices.Max(data => data.Table.Name.Length);

            var table_space = table_max_length + _padding;

            var outputs = choices
                .OrderBy(choice => choice, new TableChoiceComparer())
                .Select(choice =>
                {
                    var format =
                        $"{choice.Table.Name}".PadRight(table_space) +
                        $"{choice.Table.Schema}";
                    return (choice, format);
                });

            return outputs;
        }
    }
}
