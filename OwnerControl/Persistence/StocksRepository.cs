using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using OwnerControl.Persistence.Models;

namespace OwnerControl.Persistence
{
    public class StocksRepository : IStocksRepository
    {
        private readonly string _connectionString;
        private NpgsqlConnection Connection => new NpgsqlConnection(_connectionString);

        public StocksRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<UserStockReadModel>> GetAll(Guid userId)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                return await c.QueryAsync<UserStockReadModel>(
                    "select * from stocks where userid=@id",
                    new {id = userId}).ConfigureAwait(false);
            }
        }

        public async Task Write(Guid userId, string stock, int amount)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "insert into stocks (userid, name, amount) values (@userid, @name, @amount)",
                    new { userid = userId, name=stock, amount=amount }).ConfigureAwait(false);
            }
        }

        public async Task Delete(Guid userId, string stock, int amount)
        {
            using (var c = Connection)
            {
                try
                {
                    await c.OpenAsync().ConfigureAwait(false);
                    await c.ExecuteAsync(
                        "delete from stocks where userid=@id and name=@stock and amount=@amount",
                        new { userid = userId, name = stock, amount = amount }).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Shit went down when deleting...\n{e.Message}");
                    throw;
                }
            }
        }
    }
}