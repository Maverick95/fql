using fql.UserInterface.Choices.Formatters;
using SqlHelper.Helpers;

namespace fql.UserInterface.Choices.Selectors
{
    public class UserInputChoiceSelector : IChoiceSelector<string>
    {
        private readonly IStream _stream;

        public UserInputChoiceSelector(IStream stream)
        {
            _stream = stream;
        }

        public IEnumerable<string> Choose(IEnumerable<string> choices, IChoiceFormatter<string> formatter)
        {
            // A selector that ignores the choices and formatter and just retrieves a line of input from the user.
            _stream.Write("> ");
            var command = _stream.ReadLine();
            _stream.Padding();
            return new List<string> { command };
        }
    }
}
