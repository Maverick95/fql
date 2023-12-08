using fql.UserInterface.Choices.Selectors;
using Microsoft.Extensions.Configuration;
using SqlHelper.Config;
using SqlHelper.Contexts;
using SqlHelper.Helpers;
using System.Text.Json.Serialization;

namespace fql.Contexts
{
    public class AppSettingsContextFactory : IContextFactory
    {
        private static readonly string SECTION_CHOICE_SELECTOR = "choice-selector";

        private sealed class ChoiceSelectorSettings
        {
            [JsonPropertyName("interface")]
            public string Interface { get; set; }

            [JsonPropertyName("fzfPath")]
            public string FzfPath { get; set; }
        }

        private readonly IConfigurationRoot _configuration;

        public AppSettingsContextFactory()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();
        }

        public IConfigManager CreateConfigManager() => new AppResourceConfigManager(new FileManager(), new AppResourceConfigLocation());

        public IStream CreateStream() => new ConsoleStream();

        public IUniqueIdProvider CreateUniqueIdProvider() => new SequentialUniqueIdProvider();

        public IChoiceSelector<T> CreateChoiceSelector<T>()
        {
            IChoiceSelector<T> selector = null;
            var choiceSelectorSettings = _configuration.GetSection(SECTION_CHOICE_SELECTOR).Get<ChoiceSelectorSettings>();

            if (choiceSelectorSettings is not null)
            {
                switch (choiceSelectorSettings.Interface)
                {
                    case "fzf":
                        {
                            if (choiceSelectorSettings.FzfPath is not null &&
                                File.Exists(Path.Combine(choiceSelectorSettings.FzfPath, "fzf.exe")))
                            {
                                selector = new FzfChoiceSelector<T>(choiceSelectorSettings.FzfPath);
                            }
                        }
                        break;
                }
            }

            // Default option (same as StandardContextFactory)
            selector ??= new NumberedListChoiceSelector<T>(new ConsoleStream(), padding: 3);

            return selector;
        }
    }
}