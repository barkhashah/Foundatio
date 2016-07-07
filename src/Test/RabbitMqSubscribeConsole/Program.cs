using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Foundatio.RabbitMq.Messaging;
using Foundatio.RabbitMq.Data;

namespace Foundatio.RabbitMqSubscribeConsole {
    class Program {
        //public interface ISimpleMessage {
        //    string Data { get; set; }
        //}
        static void Main(string[] args) {
            IMessageBus _messageBus = new RabbitMqMessageService("guest", "guest", "AmazonQueue", "AmazonQueueRoutingKey", "AmazonExchange", true);
            Console.WriteLine("Subscriber....");
            _messageBus.Subscribe<ISimpleMessage>(msg => {
                Console.WriteLine(msg.Data);
            });
            Console.ReadLine();
        }
    }
}
