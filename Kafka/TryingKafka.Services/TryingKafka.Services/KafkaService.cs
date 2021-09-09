using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TryingKafka.KafkaService
{
    public class KafkaService
    {
        private IProducer<Null, string> _producerBuilder;
        private IConsumer<Null, string> _consumerBuilder;
        private CancellationTokenSource _cancellationTokenSource;

        public KafkaService(IConfiguration configuration)
        {
            var producer = AppSettings.GetConfig(configuration, "Producer");
            var consumer = AppSettings.GetConfig(configuration, "Consumer");

            if (producer != null && producer.Count > 0)
            {
                foreach (var pair in producer)
                {
                    ConsoleWriter.Information($"   Producer: {pair.Key}-{pair.Value}");
                }

                var _producerConfig = new ProducerConfig()
                {
                    BootstrapServers = producer?.Where(x => x.Key.Equals("BootStrapServers")).FirstOrDefault().Value
                };

                _producerBuilder = new ProducerBuilder<Null, string>(_producerConfig).Build();
            }

            if (consumer != null && consumer.Count > 0)
            {
                foreach (var pair in consumer)
                {
                    ConsoleWriter.Information($"   Consumer: {pair.Key}-{pair.Value}");
                }

                var _consumerConfig = new ConsumerConfig()
                {
                    BootstrapServers = consumer?.Where(x => x.Key.Equals("BootStrapServers")).FirstOrDefault().Value,
                    GroupId = consumer?.Where(x => x.Key.Equals("GroupId")).FirstOrDefault().Value,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                _consumerBuilder = new ConsumerBuilder<Null, string>(_consumerConfig).Build();

                _consumerBuilder.Subscribe(consumer?.Where(x => x.Key.Equals("TopicName")).FirstOrDefault().Value);

                ConsoleWriter.Information($"     Subscribed to {consumer?.Where(x => x.Key.Equals("TopicName")).FirstOrDefault().Value}");

                _cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    _cancellationTokenSource.Cancel();
                };
            }
        }


        public async Task<(DeliveryResult<Null, string>, string)> Publish(string topicName, string data)
        {
            try
            {
                return (await _producerBuilder.ProduceAsync(topicName, new Message<Null, string> { Value = data }), string.Empty);
            }
            catch (ProduceException<Null, String> e)
            {
                return (null, e.Error.Reason);
            }
        }

        public (ConsumeResult<Null, string>, string) Subscribe()
        {
            try
            {
                return (_consumerBuilder.Consume(_cancellationTokenSource.Token), string.Empty);
            }
            catch (ConsumeException e)
            {
                return (null, e.Error.Reason);
            }
        }
    }
}
