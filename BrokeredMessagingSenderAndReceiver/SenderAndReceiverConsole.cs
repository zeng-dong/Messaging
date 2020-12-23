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

namespace BrokeredMessagingSenderAndReceiver
{
    class SenderAndReceiverConsole
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
                Sending();
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

        static void Sending()
        {
            var queueClient = new QueueClient(ConnectionString, QueuePath);

            for (int i = 100; i < 103; i++)
            {
                var content = $"S&Rmsg: { i }";

                var message = new Message(Encoding.UTF8.GetBytes(content));
                queueClient.SendAsync(message).Wait();

                Console.WriteLine("Sent: " + i);
            }

            queueClient.CloseAsync().Wait();
        }

        static void Receiving()
        {
            var queueClient = new QueueClient(ConnectionString, QueuePath);
            queueClient.RegisterMessageHandler(ProcessMessageAsync, HandleExceptionsAsync);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();

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
                .AddUserSecrets(typeof(SenderAndReceiverConsole).Assembly)
                .Build();

            var services = ConfigureServicesProvider(config);
            services.AddSingleton(config);
            _servicesProvider = services.BuildServiceProvider();

            var lc = new LoggerConfiguration().MinimumLevel.Information();
            Log.Logger = lc
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("BrokeredMessaging.SenderAndReceiverConsole", LogEventLevel.Debug)
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
