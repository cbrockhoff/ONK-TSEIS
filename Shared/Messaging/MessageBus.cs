using System;
using System.Text;
using System.Threading.Tasks;
using Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;

namespace Shared.Messaging
{
    public class MessageBus : IMessageBus
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly string _service;

        public MessageBus(IConnectionFactory connectionFactory, ILogger logger, string service)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _service = service;
        }

        public async Task Publish(IEvent @event)
        {
            byte[] body = null;
            await Task.Run(() =>
            {
                body = SerializeMessage(@event);
                using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var exchange = DeclareEventsExchange(channel);
                    channel.BasicPublish(
                        exchange: exchange,
                        routingKey: @event.GetType().Name,
                        basicProperties: null,
                        body: body);
                }
            });
            await _logger.Information(Guid.Empty, $"Successfully published message {Encoding.UTF8.GetString(body)}");
        }

        public IDisposable Subscribe<TMessage>(Func<TMessage, Task> onEvent) where TMessage : IEvent
        {
            var channel = _connectionFactory.CreateConnection().CreateModel();
            var exchange = DeclareEventsExchange(channel);
            var queueName = channel.QueueDeclare(_service).QueueName; 
            channel.QueueBind(
                queue: queueName,
                exchange: exchange,
                routingKey: typeof(TMessage).Name);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (o, args) =>
            {
                var message = DeserializeMessage<TMessage>(args.Body);
                onEvent(message).Wait();
                _logger.Information(Guid.Empty, $"Successfully handled message {message}").GetAwaiter().GetResult();
            };

            channel.BasicConsume(
                queue: queueName,
                noAck: false,
                consumer: consumer);

            return channel;
        }

        public async Task Send(ICommand command)
        {
            byte[] body = null;
            await Task.Run(() =>
            {
                body = SerializeMessage(command);
                using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var queue = DeclareQueue(channel, command.GetType());

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queue,
                        basicProperties: null,
                        body: body);
                }
            });
            await _logger.Information(Guid.Empty, $"Successfully published message {Encoding.UTF8.GetString(body)}");
        }

        public IDisposable Receive<TMessage>(Func<TMessage, Task> onCommand) where TMessage : ICommand
        {
            var channel = _connectionFactory.CreateConnection().CreateModel();
            var queue = DeclareQueue(channel, typeof(TMessage));

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (o, args) =>
            {
                var message = DeserializeMessage<TMessage>(args.Body);
                onCommand(message).Wait();
                _logger.Information(Guid.Empty, $"Successfully handled message {message}").GetAwaiter().GetResult();
            };

            channel.BasicConsume(
                queue: queue,
                noAck: false,
                consumer: consumer);

            return channel;
        }

        private static byte[] SerializeMessage(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(json);
        }

        private static TMessage DeserializeMessage<TMessage>(byte[] body)
        {
            var json = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Deserializing {json}");
            return JsonConvert.DeserializeObject<TMessage>(json);
        }

        private static string DeclareEventsExchange(IModel channel)
        {
            const string exchange = "TSEIS.Events";
            channel.ExchangeDeclare(exchange, type: "direct");
            return exchange;
        }

        private static string DeclareQueue(IModel channel, Type commandType)
        {
            var queue = commandType.FullName;
            channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false); 
            return queue;
        }
    }
}