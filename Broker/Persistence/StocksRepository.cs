using System;
using System.Threading.Tasks;
using Dapper;
using Shared.Persistence;

namespace Broker.Persistence
{
    public class StocksRepository : BasePostgresRepository, IStocksRepository
    {
        public StocksRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task Write(Guid userId, string stock, int amount)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "insert into stocks (ownerid, stock, amount) values (@id, @stock, @amount)",
                    new {id=userId, stock=stock, amount=amount}).ConfigureAwait(false);
            }
        }

        public async Task<int> GetAmount(Guid userId, string stock)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                return await c.QueryFirstOrDefaultAsync<int>(
                    "select amount from stocks where ownerid=@id and stock=@stock",
                    new { id = @userId, stock = stock }).ConfigureAwait(false);
            }
        }

        public async Task Update(Guid userId, string stock, int amount)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "update stocks set amount=@amount where ownerid=@id and stock=@stock",
                    new { amount = amount, id = @userId, stock = stock }).ConfigureAwait(false);
            }
        }

        public async Task Delete(Guid userId, string stock, int amount)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "delete from stocks where ownerid=@id and stock=@stock and amount=@amount",
                    new { id = @userId, stock = stock, amount = amount }).ConfigureAwait(false);
            }
        }

    }
}