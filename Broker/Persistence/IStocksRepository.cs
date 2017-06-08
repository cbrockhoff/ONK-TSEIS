using System;
using System.Threading.Tasks;

namespace Broker.Persistence
{
    public interface IStocksRepository
    {
        Task Delete(Guid userId, string stock, int amount);
        Task<int> GetAmount(Guid userId, string stock);
        Task Update(Guid userId, string stock, int amount);
        Task Write(Guid userId, string stock, int amount);
    }
}