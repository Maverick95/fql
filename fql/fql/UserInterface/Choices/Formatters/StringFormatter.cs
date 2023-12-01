namespace fql.UserInterface.Choices.Formatters
{
    public class StringFormatter : IChoiceFormatter<string>
    {
        public IEnumerable<string> Format(IEnumerable<string> choices)
        {
            return choices.Select(choice => choice);
        }
    }
}
