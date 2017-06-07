using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Broker.Persistence.Models;
using Dapper;
using Shared.Persistence;

namespace Broker.Persistence
{
    public class StocksForSaleRepository : BasePostgresRepository, IStocksForSaleRepository
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

        public async Task Delete(Guid sellerId, string stockName, int amount, decimal price)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "delete from forsale where sellerid=@sellerid and stock=@stock and amount=@amount and price=@price",
                    new {sellerid = sellerId, stock = stockName, amount = amount, price = price}).ConfigureAwait(false);
            }
        }

        public async Task Write(Guid sellerId, string stockName, int amount, decimal price)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "insert into forsale (sellerid, stock, price, amount) values (@sellerid, @stock, @price, @amount)",
                    new {sellerid = sellerId, stock = stockName, price = price, amount = amount}).ConfigureAwait(false);
            }
        }
    }
}