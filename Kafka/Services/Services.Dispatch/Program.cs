using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TryingKafka.KafkaService;
using TryingKafka.KafkaService.Models;

namespace Services.Dispatch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Dispatch Service");

            ConsumeResult<Null, string> subResult;
            DeliveryResult<Null, string> pubResult;
            string error;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true).Build();

            var producerReportedTopicName = AppSettings.GetTopicName(configuration, "Reported");

            var kafkaService = new KafkaService(configuration);

            while (true)
            {
                (subResult, error) = kafkaService.Subscribe();

                if (error == string.Empty)
                {
                    var order = JsonSerializer.Deserialize<Order>(subResult.Message.Value);
                    ConsoleWriter.SubscribeReceived($"Order arrived [{order}]");

                    var report = DoDispatch(order);

                    string jsonData = JsonSerializer.Serialize(report);
                    (pubResult, error) = await kafkaService.Publish(producerReportedTopicName, jsonData);
                    ConsoleWriter.PublishValid($"Valid Order [{order}] Published to [{producerReportedTopicName}]");
                }
            }
        }

        private static Report DoDispatch(Order order)
        {
            Thread.Sleep(1000);

            return new Report()
            {
                Id = Guid.NewGuid(),
                Order = order,
                Details = "Order has been dispatched",
                Status = Status.OrderDispatched,
                CreatedOn = DateTime.Now
            };
        }
    }
}
