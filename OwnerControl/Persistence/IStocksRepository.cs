using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OwnerControl.Persistence.Models;

namespace OwnerControl.Persistence
{
    public interface IStocksRepository
    {
        Task<IEnumerable<UserStockReadModel>> GetAll(Guid userId);
        Task Write(Guid userId, string stock, int amount);
        Task Delete(Guid userId, string stock, int amount);
    }
}