using System;
using System.Threading.Tasks;

namespace Requester.Persistence
{
    public interface IStocksForSaleWriteRepository
    {
        Task Write(string stockName, int amount, decimal price);
    }
}