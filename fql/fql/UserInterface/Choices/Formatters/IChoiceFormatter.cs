namespace fql.UserInterface.Choices.Formatters
{
    public interface IChoiceFormatter<T>
    {
        public IEnumerable<(T choice, string format)> Format(IEnumerable<T> choices);
    }
}
