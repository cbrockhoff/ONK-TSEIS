using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Broker.Persistence.Models;
using Dapper;
using Shared.Persistence;

namespace Broker.Persistence
{
    public class BuyOfferRepository : BasePostgresRepository, IBuyOfferRepository
    {
        public BuyOfferRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<IEnumerable<BuyOfferReadModel>> Read()
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                return await c.QueryAsync<BuyOfferReadModel>("select * from forsale").ConfigureAwait(false);
            }
        }

        public async Task Write(Guid buyerid, string stockName, int amount, decimal price)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync("insert into buyoffers (buyerid, stock, price, amount) values (@buyerid, @stock, @price, @amount)",
                    new { buyerid = buyerid, stock = stockName, price = price, amount = amount}).ConfigureAwait(false);
            }
        }

        public async Task Delete(Guid buyerId, string stockName, int amount, decimal price)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "delete from buyoffers where buyerid=@buyerid and stock=@stock and amount=@amount and price=@price",
                    new { buyerid = buyerId, stock = stockName, amount = amount, price = price }).ConfigureAwait(false);
            }
        }
    }
}