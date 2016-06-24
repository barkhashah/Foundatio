using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundatio.Messaging;
using Foundatio.Logging;
using Foundatio.RabbitMq.Messaging;

namespace RabbitMqPublishConsole {
    public interface ISimpleMessage {
        string Data { get; set; }
    }
    public class SimpleMessageA : ISimpleMessage {
        public string Data { get; set; }
        public int Count { get; set; }
    }

    class Program {
        static void Main(string[] args) {
            IMessageBus _messageBus = new RabbitMqMessageService("guest", "guest", "AmazonQueue", "AmazonQueueRoutingKey", "AmazonExchange", true);
            _messageBus.PublishAsync(new SimpleMessageA {Data = "Sending the message 1"});
        }
    }
}
