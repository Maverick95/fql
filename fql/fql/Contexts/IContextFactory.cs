using fql.UserInterface.Choices.Selectors;
using SqlHelper.Config;
using SqlHelper.Helpers;

namespace SqlHelper.Contexts
{
    public interface IContextFactory
    {
        public IUniqueIdProvider CreateUniqueIdProvider();

        public IConfigManager CreateConfigManager();

        public IStream CreateStream();

        public IChoiceSelector<T> CreateChoiceSelector<T>();

        public IChoiceSelector<string> CreateCommandSelector();

        public IChoiceSelector<string> CreatePathSelector();
    }
}
