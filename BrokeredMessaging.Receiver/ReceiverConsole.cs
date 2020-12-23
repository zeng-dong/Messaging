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
        static string TicketCancellationQueuePath = "ticket-cancellation";

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
                //Receiving(QueuePath);
                Receiving(TicketCancellationQueuePath);

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

        static void Receiving(string queuePath)
        {
            Console.WriteLine("Receiveing from this queue: " + queuePath);
            // Create a queue client
            var queueClient = new QueueClient(ConnectionString, queuePath);

            // Create a message handler to receive messages
            queueClient.RegisterMessageHandler(ProcessMessageAsync, HandleExceptionsAsync);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();

            // Close the client
            queueClient.CloseAsync().Wait();
        }

        /*  this is the message I receive from the ticket-cancellation queue
         
Receiveing from this queue: ticket-cancellation
Press enter to exit.
Received: {
  "messageId": "91310000-e49d-ccf9-02bb-08d8a7867d10",
  "conversationId": "91310000-e49d-ccf9-a3bb-08d8a7867d15",
  "sourceAddress": "sb://globoticketz.servicebus.windows.net/DESKTOPQB0EFH1_MiniBiteApi_bus_1raoyy8ruzgxuzjzbdckxbuinh?autodelete=300",
  "destinationAddress": "sb://globoticketz.servicebus.windows.net/ticket-cancellation",
  "messageType": [
    "urn:message:MiniBite.Api.Sales.Entities:TicketCancellation"
  ],
  "message": {
    "ticketId": "202446e9-1fbe-4c1e-97d0-d93905cdd07f",
    "cancellationId": "b7c0e9fa-802b-4efb-bb3c-3c08380a93d8"
  },
  "sentTime": "2020-12-23T21:05:33.8582715Z",
  "headers": {
    "MT-Activity-Id": "|e4961366-426f573e06965f93.1."
  },
  "host": {
    "machineName": "DESKTOP-QB0EFH1",
    "processName": "MiniBite.Api",
    "processId": 46960,
    "assembly": "MiniBite.Api",
    "assemblyVersion": "1.0.0.0",
    "frameworkVersion": "3.1.10",
    "massTransitVersion": "7.0.7.0",
    "operatingSystemVersion": "Microsoft Windows NT 6.2.9200.0"
  }
}        */


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
