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
    }
}
