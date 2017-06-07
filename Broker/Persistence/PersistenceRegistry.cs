using StructureMap;

namespace Broker.Persistence
{
    public class PersistenceRegistry : Registry
    {
        public PersistenceRegistry(string connectionString)
        {
            Scan(r =>
            {
                r.TheCallingAssembly();
                r.WithDefaultConventions();

                For<IStocksForSaleRepository>()
                    .Use<StocksForSaleRepository>()
                    .Ctor<string>()
                    .Is(connectionString);
                For<IBuyOfferRepository>()
                    .Use<BuyOfferRepository>()
                    .Ctor<string>()
                    .Is(connectionString);
            });
        }
    }
}