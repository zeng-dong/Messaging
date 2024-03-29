﻿using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TryingKafka.KafkaService;
using TryingKafka.KafkaService.Models;

namespace Services.Payment
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleWriter.Information("Payment Service.");

            ConsumeResult<Null, string> subResult;
            DeliveryResult<Null, string> pubResult;
            string error;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true).Build();

            var producerProcessedTopicName = AppSettings.GetTopicName(configuration, "Processed");
            var producerReportedTopicName = AppSettings.GetTopicName(configuration, "Reported");

            var kafkaService = new KafkaService(configuration);

            while (true)
            {
                (subResult, error) = kafkaService.Subscribe();

                if (error == string.Empty)
                {
                    var order = JsonSerializer.Deserialize<Order>(subResult.Message.Value);
                    ConsoleWriter.SubscribeReceived($"Order arrived [{order}]");

                    var (report, isProcessed) = DoPaymentProcess(order);

                    string jsonData = JsonSerializer.Serialize(report);
                    (pubResult, error) = await kafkaService.Publish(producerReportedTopicName, jsonData);
                    Console.WriteLine($"Report [{report}] Published to Topic [{producerReportedTopicName}]");

                    if (isProcessed)
                    {
                        (pubResult, error) = await kafkaService.Publish(producerProcessedTopicName, subResult.Message.Value);
                        ConsoleWriter.PublishValid($"Processed Order [{order}] Published to [{producerProcessedTopicName}]");
                    }
                    else
                    {
                        ConsoleWriter.ReportInvalid($"UNProcessed Order [{order}] NOT Published to [{producerProcessedTopicName}]");
                    }
                }
            }
        }

        private static (Report, bool) DoPaymentProcess(Order order)
        {
            var isProcessed = false;
            Thread.Sleep(1);

            var report = new Report()
            {
                Id = Guid.NewGuid(),
                Order = order,
                CreatedOn = DateTime.Now
            };

            if (order.Price > 50)
            {
                report.Details = "Order has NOT been processed due to failed payment";
                report.Status = Status.PaymentFailed;
            }
            else
            {
                report.Details = "Order payment has been processed.";
                report.Status = Status.PaymentProcessed;

                isProcessed = true;
            }

            return (report, isProcessed);
        }
    }
}
