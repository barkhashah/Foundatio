using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Foundatio.Logging;
using Foundatio.RabbitMq;
using Foundatio.RabbitMq.Messaging;
using Foundatio.RabbitMq.Data;
namespace Foundatio.RabbitMqPublishConsole {
    //public interface ISimpleMessage {
    //    string Data { get; set; }
    //}


    class Program {
        static void Main(string[] args) {
            IMessageBus _messageBus = new RabbitMqMessageService("guest", "guest", "AmazonQueue",
                "AmazonQueueRoutingKey", "AmazonExchange", true, true, TimeSpan.FromMilliseconds(50));
            string input;
            Console.WriteLine("Publisher....");
            Console.WriteLine("Enter the messages to send in new lines for the AmazonExchange / AmazonQueue");
            do
            {
                input = Console.ReadLine();
                _messageBus.PublishAsync(new SimpleMessageA() { Data = input });
            } while (input != null);
            
        }
    }
}
