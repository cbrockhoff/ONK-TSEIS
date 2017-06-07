using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Broker.Persistence.Models;

namespace Broker.Persistence
{
    public interface IStocksForSaleRepository
    {
        Task Write(Guid sellerId, string stockName, int amount, decimal price);
        Task<IEnumerable<StockForSaleReadModel>> Read();
        Task Delete(Guid sellerId, string stockName, int amount, decimal price);
    }
}