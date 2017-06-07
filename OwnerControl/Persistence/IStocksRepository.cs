using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OwnerControl.Persistence.Models;

namespace OwnerControl.Persistence
{
    public interface IStocksRepository
    {
        Task<IEnumerable<UserStockReadModel>> GetAll(Guid userId);
    }
}