using StructureMap;

namespace Requester.Persistence
{
    public class PersistenceRegistry : Registry
    {
        public PersistenceRegistry(string connectionString)
        {
            Scan(r =>
            {
                r.TheCallingAssembly();
                r.WithDefaultConventions();

                For<IStocksForSaleReadRepository>()
                    .Use<StocksForSaleRepository>()
                    .Ctor<string>()
                    .Is(connectionString);
                For<IStocksForSaleWriteRepository>()
                    .Use<StocksForSaleRepository>()
                    .Ctor<string>()
                    .Is(connectionString);
            });
        }
    }
}