using SqlHelper.Config;
using SqlHelper.Helpers;

namespace SqlHelper.Contexts
{
    public interface IContextFactory
    {
        public IUniqueIdProvider CreateUniqueIdProvider();

        public IConfigManager CreateConfigManager();

        public IStream CreateStream();
    }

    public class StandardContextFactory: IContextFactory
    {
        public IUniqueIdProvider CreateUniqueIdProvider() => new SequentialUniqueIdProvider();

        public IConfigManager CreateConfigManager() => new AppResourceConfigManager(new FileManager(), new AppResourceConfigLocation());

        public IStream CreateStream() => new ConsoleStream();
    }
}
