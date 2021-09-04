using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Text;

namespace BrokeredMessaging.Sender
{
    class SenderConsole
    {
        private static IServiceProvider _servicesProvider;

        static string ConnectionString = "";
        static string QueuePath = "pl-chat-demo-queue";

        static void Main(string[] args)
        {
            SetUp();

            Run();
        }

        static void Run()
        {
            try
            {
                var sr = _servicesProvider.GetService<SettingsReader>();
                ConnectionString = sr.Settings.ConnectionString;

                // Create a queue client
                var queueClient = new QueueClient(ConnectionString, QueuePath);

                // Send some messages
                for (int i = 21; i < 27; i++)
                {
                    var content = $"Message: { i }";

                    var message = new Message(Encoding.UTF8.GetBytes(content));
                    queueClient.SendAsync(message).Wait();

                    Console.WriteLine("Sent: " + i);
                }

                // Close the client
                queueClient.CloseAsync().Wait();

                Console.WriteLine("Sent messages...");
                Console.ReadLine();

                DisposeServices();
                Log.Information("Finished Run().");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occurred.");
            }
            finally
            {
                Log.CloseAndFlush();

                DisposeServices();
                Log.Information("Finishing Run() finally.");
            }
        }

        static void SetUp()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets(typeof(SenderConsole).Assembly)
                .Build();

            var services = ConfigureServicesProvider(config);
            services.AddSingleton(config);
            _servicesProvider = services.BuildServiceProvider();

            var lc = new LoggerConfiguration().MinimumLevel.Information();
            Log.Logger = lc
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("InConsoleApp", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    "log.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 1_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                 )
                .CreateLogger();
        }

        private static IServiceCollection ConfigureServicesProvider(IConfiguration config)
        {
            var services = new ServiceCollection();

            services.AddTransient<SettingsReader>();

            services.AddLogging(configure =>
                configure.AddSerilog()
            )
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug)
            ;

            return services;
        }

        private static void DisposeServices()
        {
            if (_servicesProvider != null)
            {
                if (_servicesProvider is IDisposable)
                {
                    ((IDisposable)_servicesProvider).Dispose();
                }
            }
        }
    }
}
