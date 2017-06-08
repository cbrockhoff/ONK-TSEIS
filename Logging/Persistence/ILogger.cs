using System;
using System.Threading.Tasks;

namespace Logging
{
    public interface ILogger
    {
        Task Information(Guid correlationId, string message);
        Task Error(Guid correlationId, string message);
    }
}