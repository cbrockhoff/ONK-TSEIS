using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;

namespace Shared.Messaging
{
    public class MessageBus : IMessageBus, IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger _logger;
        private readonly string _service;
        private readonly List<IDisposable> _connections  = new List<IDisposable>();

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
            await _logger.Information(Guid.Empty, $"Successfully published {@event.GetType().Name} with body {Encoding.UTF8.GetString(body)}");
        }

        public IDisposable Subscribe<TMessage>(Func<TMessage, Task> onEvent) where TMessage : IEvent
        {
            try
            {
                var channel = _connectionFactory.CreateConnection().CreateModel();
                var exchange = DeclareEventsExchange(channel);
                var queueName = channel.QueueDeclare(
                    $"{_service}.{typeof(TMessage).Name}", 
                    exclusive: false ).QueueName; 
                channel.QueueBind(
                    queue: queueName,
                    exchange: exchange,
                    routingKey: typeof(TMessage).Name);

                var consumer = CreateConsumer(channel, onEvent);

                channel.BasicConsume(
                    queue: queueName,
                    noAck: false,
                    consumer: consumer);

                _connections.Add(channel);
                Console.WriteLine($"Subscribed to {typeof(TMessage).Name}");
                return channel;
            }
            catch (Exception e)
            {
                var msg = $"Something went wrong while trying to subscribe to {typeof(TMessage).Name} : {e.Message}";
                Console.WriteLine(msg);
                _logger.Error(Guid.Empty, msg).GetAwaiter().GetResult();
                return null;
            }
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
            await _logger.Information(Guid.Empty, $"Successfully published {command.GetType().Name} with body {Encoding.UTF8.GetString(body)}");
        }

        public IDisposable Receive<TMessage>(Func<TMessage, Task> onCommand) where TMessage : ICommand
        {
            try
            {
                var channel = _connectionFactory.CreateConnection().CreateModel();
                var queue = DeclareQueue(channel, typeof(TMessage));

                var consumer = CreateConsumer(channel, onCommand);

                channel.BasicConsume(
                    queue: queue,
                    noAck: false,
                    consumer: consumer);

                _connections.Add(channel);
                Console.WriteLine($"Receiving {typeof(TMessage).Name}");
                return channel;
            }
            catch (Exception e)
            {
                var msg = $"Something went wrong when receiving {typeof(TMessage).Name} : {e.Message}";
                Console.WriteLine(msg);
                _logger.Error(Guid.Empty, msg).GetAwaiter().GetResult();
                return null;
            }
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

        private EventingBasicConsumer CreateConsumer<TMessage>(IModel model, Func<TMessage, Task> onMessage)
        {
            var consumer = new EventingBasicConsumer(model);
            consumer.Received += (o, args) =>
            {
                try
                {
                    var message = DeserializeMessage<TMessage>(args.Body);
                    onMessage(message).GetAwaiter().GetResult();
                    _logger.Information(Guid.Empty, $"Successfully handled {typeof(TMessage).Name} with body {JsonConvert.SerializeObject(message)}").GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    var msg = $"Error handling {typeof(TMessage).Name}: {e.Message}";
                    Console.WriteLine(msg);
                    _logger.Error(Guid.Empty, msg);
                }
            };
            return consumer;
        }

        public void Dispose()
        {
            foreach (var c in _connections)
            {
                c.Dispose();
            }
        }
    }
}