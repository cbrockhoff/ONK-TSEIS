using RabbitMQ.Client;
using StructureMap;

namespace Shared.Messaging
{
    public class MessagingRegistry : Registry
    {
        public MessagingRegistry(string service)
        {
            Scan(r =>
            {
                r.TheCallingAssembly();
                r.WithDefaultConventions();
                For<IConnectionFactory>().Use(new ConnectionFactory()
                {
                    HostName = "rabbitmq" // mmh, hardcoded
                });
                For<IMessageBus>().Use<MessageBus>().Ctor<string>().Is(service).Singleton();
            });
        }
    }
}