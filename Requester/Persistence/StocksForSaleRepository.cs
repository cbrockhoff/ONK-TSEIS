using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Requester.Persistence.Models;
using Shared.Persistence;

namespace Requester.Persistence
{
    public class StocksForSaleRepository : BasePostgresRepository, IStocksForSaleReadRepository, IStocksForSaleWriteRepository
    {
        public StocksForSaleRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<IEnumerable<StockForSaleReadModel>> Read()
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                return await c.QueryAsync<StockForSaleReadModel>("select * from forsale").ConfigureAwait(false);
            }
        }

        public async Task Write(string stockName, int amount, decimal price)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "insert into forsale (name, price, amount) values (@stock, @price, @amount)",
                    new { name = stockName, price = price, amount = amount }).ConfigureAwait(false);
            }
        }
    }
}
