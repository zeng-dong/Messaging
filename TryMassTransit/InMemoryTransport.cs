using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TryMassTransit
{
    public class InMemoryTransport
    {
        public static async Task Process()
        {
            var bus = Bus.Factory.CreateUsingInMemory(sbc =>
            {
                sbc.ReceiveEndpoint("order_queue", ep =>
                {
                    ep.Handler<Message>(context =>
                    {
                        return Console.Out.WriteLineAsync($"Received: {context.Message.Text}, its order ID is {context.Message.OrderId}");
                    });

                    ep.Consumer<MessageConsumerOne>();
                    ep.Consumer<MessageConsumerTwo>();
                });
            });

            await bus.StartAsync();

            try
            {
                var index = 0;
                while (true)
                {
                    object p = bus.Publish(new Message { OrderId = index, Text = $"{DateTime.UtcNow} => message index order: {index++}" });
                    await Task.Run(() => Thread.Sleep(1000));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
            finally
            {
                await bus.StopAsync();
            }
        }
    }
}
