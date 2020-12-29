using System.Threading.Tasks;

namespace TryMassTransit
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await InMemoryTransport.Process();
        }
    }
}


/* output of InMemoryTransport.Process() 
Received: 12/26/2020 3:51:53 PM => message index order: 0, its order ID is 0
MessageConsumerOne: OrderId: 0 text: 12/26/2020 3:51:53 PM => message index order: 0
MessageConsumer-2: OrderId: 0 text: 12/26/2020 3:51:53 PM => message index order: 0
Received: 12/26/2020 3:51:54 PM => message index order: 1, its order ID is 1
MessageConsumer-2: OrderId: 1 text: 12/26/2020 3:51:54 PM => message index order: 1
MessageConsumerOne: OrderId: 1 text: 12/26/2020 3:51:54 PM => message index order: 1
Received: 12/26/2020 3:51:55 PM => message index order: 2, its order ID is 2
MessageConsumerOne: OrderId: 2 text: 12/26/2020 3:51:55 PM => message index order: 2
MessageConsumer-2: OrderId: 2 text: 12/26/2020 3:51:55 PM => message index order: 2
Received: 12/26/2020 3:51:56 PM => message index order: 3, its order ID is 3
MessageConsumer-2: OrderId: 3 text: 12/26/2020 3:51:56 PM => message index order: 3
MessageConsumerOne: OrderId: 3 text: 12/26/2020 3:51:56 PM => message index order: 3
 
 */
