using fql.UserInterface.Choices.Selectors;
using SqlHelper.Config;
using SqlHelper.Contexts;
using SqlHelper.Helpers;

namespace fql.Contexts
{
    public class StandardContextFactory : IContextFactory
    {
        public IUniqueIdProvider CreateUniqueIdProvider() => new SequentialUniqueIdProvider();

        public IConfigManager CreateConfigManager() => new AppResourceConfigManager(new FileManager(), new AppResourceConfigLocation());

        public IStream CreateStream() => new ConsoleStream();

        public IChoiceSelector<T> CreateChoiceSelector<T>() => new NumberedListChoiceSelector<T>(new ConsoleStream(), padding: 3);
    }
}