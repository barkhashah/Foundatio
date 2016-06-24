using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Extensions;
using Foundatio.Logging;
using Foundatio.Messaging;
using Foundatio.Serializer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Foundatio.RabbitMq.Messaging
{
    public class RabbitMqMessageService : MessageBusBase, IMessageBus
    {
        private readonly string _userName;
        private readonly string _password;
        private readonly string _queueName;
        private readonly string _routingKey;
        private readonly string _exchangeName;
        private readonly bool _durable;
        private readonly ILogger _log;
        private readonly ISerializer _serializer;
        public RabbitMqMessageService(string userName, string password, string queueName, string routingKey, string exhangeName, bool durable,
            ISerializer serializer = null, ILoggerFactory loggerFactory = null
            ) : base(loggerFactory)
        {
            _serializer = serializer ?? new JsonNetSerializer();
            _userName = userName;
            _password = password;
            _exchangeName = exhangeName;
            _queueName = queueName;
            _routingKey = routingKey;
            _durable = durable;
            _log = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Connect to a broker - RabbitMQ
        /// </summary>
        /// <returns></returns>
        private IConnection CreateConnection()
        {
            IConnectionFactory factory = new ConnectionFactory();
            factory.UserName = _userName;
            factory.Password = _password;

            return factory.CreateConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>RabbitMQ has an upper limit of 2GB for messages.BasicPublish blocking AMQP operations</remarks>
        /// <returns></returns>

        public override async Task PublishAsync(Type messageType, object message, TimeSpan? delay = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null)
                return;
            _logger.Trace("Message Publish: {messageType}", messageType.FullName);

            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                await AddDelayedMessageAsync(messageType, message, delay.Value).AnyContext();
                return;
            }


            using (var connection = CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // setup the message router - it requires the name of our exchange, exhange type and durability
                    // ( it will survive a server restart )
                    channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, _durable);
                    // setup the queue where the messages will reside - it requires the queue name and durability.
                    channel.QueueDeclare(_queueName, _durable, false, false, null);
                    // bind the queue to the exchange
                    channel.QueueBind(_queueName, _exchangeName, _routingKey);

                    IBasicProperties basicProperties = channel.CreateBasicProperties();
                    basicProperties.Persistent = true;

                    var data = await _serializer.SerializeAsync(new MessageBusData {
                        Type = messageType.AssemblyQualifiedName,
                        Data = await _serializer.SerializeToStringAsync(message).AnyContext()
                    }).AnyContext();

                    channel.BasicPublish(_exchangeName, _routingKey, basicProperties, data);

                }
            }
            
        }

        public override void Subscribe<T>(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, _durable);
                    // setup the queue where the messages will reside - it requires the queue name and durability.
                    channel.QueueDeclare(_queueName, _durable, false, false, null);
                   
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (o, e) =>
                    {
                        _log.Trace("OnMessage ");
                        var message = await _serializer.DeserializeAsync<MessageBusData>((byte[])e.Body).AnyContext();
                        Type messageType;
                        try {
                            messageType = Type.GetType(message.Type);
                        }
                        catch (Exception ex) {
                            _logger.Error(ex, "Error getting message body type: {0}", ex.Message);
                            return;
                        }
                        object body = await _serializer.DeserializeAsync(message.Data, messageType).AnyContext();
                        await SendMessageToSubscribersAsync(messageType, body).AnyContext();
                    };
                    var consumerTag = channel.BasicConsume(_queueName, true, consumer);
                    // bind the queue to the exchange
                    channel.QueueBind(_queueName, _exchangeName, _routingKey);

                }
            }
            base.Subscribe(handler, cancellationToken);

        }

        private async void OnMessage(object sender, BasicDeliverEventArgs e)
        {
            _log.Trace("OnMessage ");
            var message = await _serializer.DeserializeAsync<MessageBusData>((byte[])e.Body).AnyContext();
            Type messageType;
            try
            {
                messageType = Type.GetType(message.Type);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting message body type: {0}", ex.Message);
                return;
            }
            object body = await _serializer.DeserializeAsync(message.Data, messageType).AnyContext();
            await SendMessageToSubscribersAsync(messageType, body).AnyContext();
        }
    }
}
