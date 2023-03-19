using SqlHelper.Config;
using SqlHelper.Helpers;

namespace SqlHelper.Contexts
{
    public interface IContextFactory
    {
        public IUniqueIdProvider CreateUniqueIdProvider();

        public IConfigManager CreateConfigManager();
    }

    public class StandardContextFactory: IContextFactory
    {
        public IUniqueIdProvider CreateUniqueIdProvider() => new SequentialUniqueIdProvider();

        public IConfigManager CreateConfigManager() => new AppResourceConfigManager(new FileManager());
    }

    public static class Context
    {
        private static IContextFactory _contextFactory;

        private static IUniqueIdProvider _uniqueIdProvider;
        private static IConfigManager _configManager;

        static Context()
        {
            _contextFactory = new StandardContextFactory();
        }

        public static void Use(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _uniqueIdProvider = null;
            _configManager = null;
        }

        public static IUniqueIdProvider UniqueId
        {
            get
            {
                if (_uniqueIdProvider is null)
                    _uniqueIdProvider = _contextFactory.CreateUniqueIdProvider();
                return _uniqueIdProvider;
            }
        }

        public static IConfigManager Config
        {
            get
            {
                if (_configManager is null)
                    _configManager = _contextFactory.CreateConfigManager();
                return _configManager;
            }
        }
    }
}
