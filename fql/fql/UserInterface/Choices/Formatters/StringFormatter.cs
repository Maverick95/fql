namespace fql.UserInterface.Choices.Formatters
{
    public class StringFormatter : IChoiceFormatter<string>
    {
        public IEnumerable<(string choice, string format)> Format(IEnumerable<string> choices)
        {
            return choices
                .OrderBy(choice => choice, StringComparer.InvariantCultureIgnoreCase)
                .Select(choice => (choice, choice));
        }
    }
}
