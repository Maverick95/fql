using fql.UserInterface.Choices.Formatters;

namespace fql.UserInterface.Choices.Selectors
{
    public interface IChoiceSelector<T>
    {
        public IEnumerable<T> Choose(IEnumerable<T> choices, IChoiceFormatter<T> formatter);
    }
}
