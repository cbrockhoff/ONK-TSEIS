using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Broker.Persistence;
using Logging;
using Shared.Contracts.Commands;
using Shared.Contracts.Events;
using Shared.Messaging;
using StructureMap;

namespace Broker.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Broker service started...");
            var container = new Container();
            container.Configure(cfg =>
            {
                cfg.AddRegistry(new MessagingRegistry("Broker.Service"));
                cfg.AddRegistry(new PersistenceRegistry("User ID=postgres;Password=password;Host=10.0.0.90;Port=5432;Database=tseis"));
                cfg.AddRegistry(new LoggingRegistry("Broker.Service", "User ID=postgres;Password=password;Host=10.0.0.50;Port=5432;Database=tseis"));
            });

            var bus = container.GetInstance<IMessageBus>();
            var forSaleRepo = container.GetInstance<IStocksForSaleRepository>();
            var buyOfferRepo = container.GetInstance<IBuyOfferRepository>();
            var ownerRepo = container.GetInstance<IStocksRepository>();

            var receives = new[]
            {
                bus.Receive<SetStockForSaleCommandDto>(async cmd =>
                {
                    if ((await ownerRepo.GetAmount(cmd.SellerId, cmd.Stock) != cmd.Amount))
                        return;

                    await forSaleRepo.Write(cmd.SellerId, cmd.Stock, cmd.Amount, cmd.Price);
                    await bus.Publish(new StockSetForSaleEventDto()
                    {
                        Name = cmd.Stock,
                        Amount = cmd.Amount,
                        Price = cmd.Price
                    });
                }),
                bus.Receive<PlaceBuyOfferCommandDto>(cmd => buyOfferRepo.Write(cmd.BuyerId, cmd.Stock, cmd.Amount, cmd.Price)),
                bus.Subscribe<StockTradeHappenedEventDto>(async e =>
                {
                    await ownerRepo.Delete(e.SellerId, e.Stock, e.Amount);
                    await ownerRepo.Write(e.BuyerId, e.Stock, e.Amount);
                }),
                bus.Subscribe<UserReceivedStockEventDto>(e => ownerRepo.Write(e.UserId, e.Stock, e.Amount))
            };

            var timer = new Timer(o =>
            {
                MatchOffers(forSaleRepo, buyOfferRepo, bus);
            }, null, 10000, 10000);

            var hack = new ManualResetEvent(false);
            hack.WaitOne();

            timer.Dispose();
            foreach (var r in receives)
            {
                r.Dispose();
            }
        }

        static void MatchOffers(
            IStocksForSaleRepository forSaleRepo, 
            IBuyOfferRepository buyOffersRepo,
            IMessageBus bus)
        {
            var forSaleTask = forSaleRepo.Read();
            var buyOffersTask = buyOffersRepo.Read();
            Task.WhenAll(forSaleTask, buyOffersTask).GetAwaiter().GetResult();
            var stocksForSale = forSaleTask.Result.ToList();
            var buyOffers = buyOffersTask.Result.ToList();

            foreach (var offer in buyOffers)
            {
                try
                {
                    var matchingForSale = stocksForSale.FirstOrDefault(s => 
                        string.Equals(s.Stock, offer.Stock, StringComparison.CurrentCultureIgnoreCase) &&
                        s.Price == offer.Price &&
                        s.Amount == offer.Amount);

                    if (matchingForSale == null)
                        continue;

                    // made a sale, yay
                    stocksForSale.Remove(matchingForSale);
                    Task.WhenAll(
                        buyOffersRepo.Delete(offer.BuyerId, offer.Stock, offer.Amount, offer.Price),
                        forSaleRepo.Delete(matchingForSale.SellerId, matchingForSale.Stock, matchingForSale.Amount, matchingForSale.Price),
                        bus.Publish(new StockTradeHappenedEventDto()
                        {
                            BuyerId = offer.BuyerId,
                            SellerId = matchingForSale.SellerId,
                            Amount = offer.Amount,
                            Price = offer.Price,
                            Stock = offer.Stock
                        })).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Problem occured while matching offers: {e.Message}");
                    throw;
                }
            }
        }
    }
}