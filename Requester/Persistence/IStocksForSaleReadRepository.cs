using System.Collections.Generic;
using System.Threading.Tasks;
using Requester.Persistence.Models;

namespace Requester.Persistence
{
    public interface IStocksForSaleReadRepository
    {
        Task<IEnumerable<StockForSaleReadModel>> Read();
    }
}