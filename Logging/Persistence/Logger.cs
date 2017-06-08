using System;
using System.Threading.Tasks;
using Dapper;
using Shared.Persistence;

namespace Logging
{
    public class Logger : BasePostgresRepository, ILogger
    {
        private readonly string _service;

        public Logger(string connectionString, string service) : base(connectionString)
        {
            _service = service;
        }

        public Task Information(Guid correlationId, string message)
        {
            return Log(correlationId, message, "information");
        }

        public Task Error(Guid correlationId, string message)
        {
            return Log(correlationId, message, "error");
        }

        private async Task Log(Guid correlationId, string message, string level)
        {
            using (var c = Connection)
            {
                await c.OpenAsync().ConfigureAwait(false);
                await c.ExecuteAsync(
                    "insert into logs (correlationid, service, message, level, occurred) " +
                    "values (@correlationid, @service, @message, @level, @occurred)",
                    new
                    {
                        correlationid = correlationId,
                        service = _service,
                        message = message,
                        level = level,
                        occurred = DateTime.Now

                    }).ConfigureAwait(false);
            }
        }
    }
}
