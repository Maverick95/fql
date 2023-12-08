using fql.Contexts;
using fql.UserInterface.Choices.Models;
using fql.UserInterface.Choices.Selectors;
using SqlHelper.Config;
using SqlHelper.Helpers;

namespace SqlHelper.Contexts
{
    public static class Context
    {
        private static IContextFactory _contextFactory = new AppSettingsContextFactory();

        private static IUniqueIdProvider _uniqueIdProvider;
        private static IConfigManager _configManager;
        private static IStream _stream;

        private static IChoiceSelector<TableChoice> _tableChoiceSelector;
        private static IChoiceSelector<FilterChoice> _filterChoiceSelector;
        private static IChoiceSelector<CustomConstraintChoice> _customConstraintChoiceSelector;

        public static void Use(IContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _uniqueIdProvider = null;
            _configManager = null;
            _stream = null;
            _tableChoiceSelector = null;
            _filterChoiceSelector = null;
            _customConstraintChoiceSelector = null;
        }

        public static IUniqueIdProvider UniqueIdProvider
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

        public static IChoiceSelector<FilterChoice> FilterChoiceSelector
        {
            get
            {
                if (_filterChoiceSelector is null)
                    _filterChoiceSelector = _contextFactory.CreateChoiceSelector<FilterChoice>();
                return _filterChoiceSelector;
            }
        }

        public static IChoiceSelector<TableChoice> TableChoiceSelector
        {
            get
            {
                if (_tableChoiceSelector is null)
                    _tableChoiceSelector = _contextFactory.CreateChoiceSelector<TableChoice>();
                return _tableChoiceSelector;
            }
        }

        public static IChoiceSelector<CustomConstraintChoice> CustomConstraintChoiceSelector
        {
            get
            {
                if (_customConstraintChoiceSelector is null)
                    _customConstraintChoiceSelector = _contextFactory.CreateChoiceSelector<CustomConstraintChoice>();
                return _customConstraintChoiceSelector;
            }
        }
    }
}
