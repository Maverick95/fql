using SqlHelper.Config;
using SqlHelper.Helpers;

namespace SqlHelper.Contexts
{
    public static class Context
    {
        private static IContextFactory _contextFactory;

        private static IUniqueIdProvider _uniqueIdProvider;
        private static IConfigManager _configManager;
        private static IStream _stream;

        static Context()
        {
            _contextFactory = new StandardContextFactory();
        }

        public static void Use(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _uniqueIdProvider = null;
            _configManager = null;
            _stream = null;
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

        public static IStream Stream
        {
            get
            {
                if (_stream is null)
                    _stream = _contextFactory.CreateStream();
                return _stream;
            }
        }
    }
}
