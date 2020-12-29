using MassTransit;
using System;
using System.Threading.Tasks;

namespace TryMassTransit
{
    public class MessageConsumerTwo : IConsumer<Message>
    {
        public Task Consume(ConsumeContext<Message> context)
        {
            Console.Out.WriteLineAsync($"MessageConsumer-2: OrderId: {context.Message.OrderId} text: {context.Message.Text}");
            return Task.CompletedTask;
        }
    }
}
