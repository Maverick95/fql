using fql.UserInterface.Choices.Models;

namespace fql.UserInterface.Choices.Formatters
{
    public class CustomConstraintChoiceFormatter : IChoiceFormatter<CustomConstraintChoice>
    {
        private readonly int _padding;

        public CustomConstraintChoiceFormatter(int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _padding = padding;
        }

        public IEnumerable<string> Format(IEnumerable<CustomConstraintChoice> choices)
        {
            if (!choices.Any())
            {
                return new List<string>();
            }

            var source_table_max_length = choices
                .Select(data => $"{data.SourceTable.Schema}.{data.SourceTable.Name}")
                .Max(name => name.Length);

            var source_table_space = source_table_max_length + _padding;

            var outputs = choices.Select(choice =>
                $"{choice.SourceTable.Schema}.{choice.SourceTable.Name}".PadRight(source_table_space) +
                $"<----".PadRight(5 + _padding) +
                $"{choice.TargetTable.Schema}.{choice.TargetTable.Name}");

            return outputs;
        }
    }
}
