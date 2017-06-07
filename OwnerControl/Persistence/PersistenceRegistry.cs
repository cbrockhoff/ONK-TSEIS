using StructureMap;

namespace OwnerControl.Persistence
{
    public class PersistenceRegistry : Registry
    {
        public PersistenceRegistry(string connectionString)
        {
            Scan(r =>
            {
                r.TheCallingAssembly();
                r.WithDefaultConventions();
                For<IStocksRepository>().Use<StocksRepository>().Ctor<string>().Is(connectionString);
            });
        }
    }
}