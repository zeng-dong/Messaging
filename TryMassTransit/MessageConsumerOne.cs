using MassTransit;
using System;
using System.Threading.Tasks;

namespace TryMassTransit
{
    public class MessageConsumerOne : IConsumer<Message>
    {
        public Task Consume(ConsumeContext<Message> context)
        {
            Console.Out.WriteLineAsync($"MessageConsumerOne: OrderId: {context.Message.OrderId} text: {context.Message.Text}");

            return Task.CompletedTask;
        }
    }
}
