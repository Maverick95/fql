namespace fql.UserInterface.Choices.Formatters
{
    public interface IChoiceFormatter<T>
    {
        public IEnumerable<string> Format(IEnumerable<T> choices);
    }
}
