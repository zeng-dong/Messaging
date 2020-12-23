using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrokeredMessaging.Receiver
{
    class ReceiverConsole
    {
        private static IServiceProvider _servicesProvider;

        static string ConnectionString = "";
        static string QueuePath = "chat-demo-queue";
        static void Main(string[] args)
        {
            SetUp();

            Run();
        }

        static void Run()
        {
            try
            {
                SetConnectionString();
                Receiving();

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

        static void SetConnectionString()
        {
            var sr = _servicesProvider.GetService<SettingsReader>();
            ConnectionString = sr.Settings.ConnectionString;
            Log.Information($"Receiving messages on this connection: {ConnectionString}");
        }

        static void Receiving()
        {
            // Create a queue client
            var queueClient = new QueueClient(ConnectionString, QueuePath);

            // Create a message handler to receive messages
            queueClient.RegisterMessageHandler(ProcessMessageAsync, HandleExceptionsAsync);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();

            // Close the client
            queueClient.CloseAsync().Wait();
        }

        private static async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            // retrieve content from message body
            var content = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Received: {content}");
        }

        private static Task HandleExceptionsAsync(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        static void SetUp()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddUserSecrets(typeof(ReceiverConsole).Assembly)
                .Build();

            var services = ConfigureServicesProvider(config);
            services.AddSingleton(config);
            _servicesProvider = services.BuildServiceProvider();

            var lc = new LoggerConfiguration().MinimumLevel.Information();
            Log.Logger = lc
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("BrokeredMessaging.Receiver", LogEventLevel.Debug)
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
