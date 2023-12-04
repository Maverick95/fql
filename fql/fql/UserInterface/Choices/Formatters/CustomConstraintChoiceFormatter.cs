using fql.UserInterface.Choices.Models;

namespace fql.UserInterface.Choices.Formatters
{
    public class CustomConstraintChoiceFormatter : IChoiceFormatter<CustomConstraintChoice>
    {
        private sealed class CustomConstraintChoiceComparer : IComparer<CustomConstraintChoice>
        {
            public int Compare(CustomConstraintChoice x, CustomConstraintChoice y)
            {
                var compareSourceSchemas = string.Compare(x.SourceTable.Schema, y.SourceTable.Schema, StringComparison.InvariantCultureIgnoreCase);
                if (compareSourceSchemas is not 0)
                {
                    return compareSourceSchemas;
                }
                var compareSourceTables = string.Compare(x.SourceTable.Name, y.SourceTable.Name, StringComparison.InvariantCultureIgnoreCase);
                if (compareSourceTables is not 0)
                {
                    return compareSourceTables;
                }
                var compareTargetSchemas = string.Compare(x.TargetTable.Schema, y.TargetTable.Schema, StringComparison.InvariantCultureIgnoreCase);
                if (compareTargetSchemas is not 0)
                {
                    return compareTargetSchemas;
                }
                var compareTargetTables = string.Compare(x.TargetTable.Name, y.TargetTable.Name, StringComparison.InvariantCultureIgnoreCase);
                return compareTargetTables;
            }
        }

        private readonly int _padding;

        public CustomConstraintChoiceFormatter(int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _padding = padding;
        }

        public IEnumerable<(CustomConstraintChoice choice, string format)> Format(IEnumerable<CustomConstraintChoice> choices)
        {
            if (!choices.Any())
            {
                return new List<(CustomConstraintChoice, string)>();
            }

            var source_table_max_length = choices
                .Select(data => $"{data.SourceTable.Schema}.{data.SourceTable.Name}")
                .Max(name => name.Length);

            var source_table_space = source_table_max_length + _padding;

            var outputs = choices
                .OrderBy(choice => choice, new CustomConstraintChoiceComparer())
                .Select(choice =>
                {
                    var format =
                        $"{choice.SourceTable.Schema}.{choice.SourceTable.Name}".PadRight(source_table_space) +
                        $"<----".PadRight(5 + _padding) +
                        $"{choice.TargetTable.Schema}.{choice.TargetTable.Name}";
                    return (choice, format);
                });

            return outputs;
        }
    }
}
