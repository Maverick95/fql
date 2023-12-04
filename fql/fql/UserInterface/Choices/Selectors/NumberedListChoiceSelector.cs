using fql.UserInterface.Choices.Formatters;
using SqlHelper.Extensions;
using SqlHelper.Helpers;

namespace fql.UserInterface.Choices.Selectors
{
    public class NumberedListChoiceSelector<T> : IChoiceSelector<T>
    {
        private readonly IStream _stream;
        private readonly int _padding;

        public NumberedListChoiceSelector(IStream stream, int padding)
        {
            if (padding < 0)
            {
                throw new ArgumentOutOfRangeException("padding", padding, "must be non-negative");
            }
            _stream = stream;
            _padding = padding;
        }

        public IEnumerable<T> Choose(IEnumerable<T> choices, IChoiceFormatter<T> formatter)
        {
            if (!choices.Any())
            {
                return new List<T>();
            }

            var ids = Enumerable.Range(1, choices.Count());
            var id_space = choices.Count().ToString().Length + _padding;

            var formats = formatter.Format(choices);

            var options = ids.Zip(formats, (id, format) => new
            {
                Id = id,
                Choice = format.choice,
                Text = $"{id}".PadRight(id_space) + format.format,
            });

            foreach (var option in options)
            {
                _stream.WriteLine(option.Text);
            }
            _stream.Padding();
            _stream.Write("> ");
            var cleaned = _stream.ReadLine().Clean();
            _stream.Padding();

            var selected = cleaned
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Join(
                    options,
                    input => input,
                    option => option.Id.ToString(),
                    (input, option) => option.Choice);

            return selected;
        }
    }
}
