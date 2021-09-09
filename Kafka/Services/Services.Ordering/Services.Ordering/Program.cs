using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TryingKafka.KafkaService;
using TryingKafka.KafkaService.Models;

namespace Services.Ordering
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleWriter.Information("Ordering Service.");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true).Build();

            var producerSubmittedTopicName = AppSettings.GetTopicName(configuration, "Submitted");
            var producerReportedTopicName = AppSettings.GetTopicName(configuration, "Reported");

            var kafkaService = new KafkaService(configuration);

            while (true)
            {
                var (order, report) = DoOrdering();

                string jsonData = JsonSerializer.Serialize(report);
                var (result, error) = await kafkaService.Publish(producerReportedTopicName, jsonData);
                Console.WriteLine($"Report [{report}] Published to Topic [{producerReportedTopicName}]");

                jsonData = JsonSerializer.Serialize(order);
                (result, error) = await kafkaService.Publish(producerSubmittedTopicName, jsonData);

                Console.WriteLine($"Order [{order}] Published to [{producerSubmittedTopicName}]");
            }
        }

        private static (Order, Report) DoOrdering()
        {
            var rnd = new Random();
            Thread.Sleep(10000);
            var pId = rnd.Next(111111, 999999);

            var order = new Order()
            {
                Id = Guid.NewGuid(),
                ProductId = pId,
                ProductName = $"Product {pId}",
                Quantity = rnd.Next(1, 10),
                Price = rnd.Next(1, 100),
                CreatedOn = DateTime.Now
            };

            var report = new Report()
            {
                Id = Guid.NewGuid(),
                Order = order,
                Details = "Order has been submitted",
                Status = Status.OrderSubmitted,
                CreatedOn = DateTime.Now
            };

            return (order, report);
        }
    }
}
