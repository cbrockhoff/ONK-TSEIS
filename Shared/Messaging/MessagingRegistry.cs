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
                For<ConnectionFactory>().Use(new ConnectionFactory()
                {
                    HostName = "10.0.0.100" // mmh, hardcoded
                });
                For<IMessageBus>().Use<MessageBus>().Ctor<string>().Is(service).Singleton();
            });
        }
    }
}