using System;
using System.Threading;
using Logging;
using Shared.Contracts.Events;
using Shared.Messaging;
using StructureMap;

namespace TobinTaxer.Service
{
    class Program
    {
        private const string Service = "TobinTaxer.Service";

        static void Main(string[] args)
        {
            Console.WriteLine("TobinTaxer service started...");

            var container = new Container();
            container.Configure(cfg =>
            {
                cfg.AddRegistry(new MessagingRegistry(Service));
                cfg.AddRegistry(new LoggingRegistry(Service, "User ID=postgres;Password=password;Host=10.0.0.50;Port=5432;Database=tseis"));
            });

            var bus = container.GetInstance<IMessageBus>();
            var logger = container.GetInstance<ILogger>();

            var recv = bus.Subscribe<StockTradeHappenedEventDto>(e => 
                logger.Information(Guid.Empty, $"Pretend I taxed the transaction of stock {e.Stock} from seller {e.SellerId} to buyer {e.BuyerId}"));

            var hack = new ManualResetEvent(false);
            hack.WaitOne();

            recv.Dispose();
        }
    }
}