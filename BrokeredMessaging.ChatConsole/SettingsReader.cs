using Microsoft.Extensions.Configuration;
using Serilog;

namespace BrokeredMessaging.ChatConsole
{
    public class SettingsReader
    {
        private readonly IConfiguration config;
        public Settings Settings { get; }

        public SettingsReader(IConfiguration config)
        {
            this.config = config;
            this.Settings = this.config.GetSection("AzureServiceBus").Get<Settings>();
        }
        public void DescribeSettings()
        {
            Log.Information($"SettingsReader -> settings -> Password:  ({ Settings.Password})");
            Log.Information($"SettingsReader -> settings -> ConnectionString:  ({ Settings.ConnectionString})");
        }
    }
}
