using Npgsql;

namespace Shared.Persistence
{
    public abstract class BasePostgresRepository
    {
        protected NpgsqlConnection Connection => new NpgsqlConnection(_connectionString);
        private readonly string _connectionString;

        protected BasePostgresRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}
