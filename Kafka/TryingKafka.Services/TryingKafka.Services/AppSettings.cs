using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace TryingKafka.Services
{
    public static class AppSettings
    {
        private const string Prefix = "KafkaSettings";

        public static List<KeyValuePair<string, string>> GetConfig(IConfiguration configuration, string key)
        {
            return configuration.GetSection($"{Prefix}:{key}").GetChildren()
                .ToDictionary(x => x.Key, x => x.Value).ToList();
        }

        public static string GetTopicName(IConfiguration configuration, string key)
        {
            return configuration.GetSection($"{Prefix}:Producer:{key}").GetChildren()
                .ToDictionary(x => x.Key, x => x.Value).ToList()?
                .Where(x => x.Key.Equals("TopicName")).FirstOrDefault().Value;
        }
    }
}
