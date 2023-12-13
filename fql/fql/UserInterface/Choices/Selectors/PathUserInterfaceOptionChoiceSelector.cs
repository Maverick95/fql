using fql.UserInterface.Choices.Formatters;
using SqlHelper.Extensions;
using SqlHelper.Helpers;

namespace fql.UserInterface.Choices.Selectors
{
    public class PathUserInterfaceOptionChoiceSelector : IChoiceSelector<string>
    {
        private readonly IStream _stream;

        public PathUserInterfaceOptionChoiceSelector(IStream stream)
        {
            _stream = stream;
        }

        public IEnumerable<string> Choose(IEnumerable<string> choices, IChoiceFormatter<string> formatter)
        {
            var options = choices.ToHashSet();
            var selected = new List<string>();

            _stream.Padding();
            _stream.Write("> ");

            while (!selected.Any())
            {
                var input = _stream.ReadLine();
                _stream.Padding();
                var cleaned = input.Clean();

                if (options.Contains(cleaned))
                {
                    selected.Add(cleaned);
                }
                else
                {
                    _stream.Write("Wrong choice, try again : ");
                }
            }

            return selected;
        }
    }
}
