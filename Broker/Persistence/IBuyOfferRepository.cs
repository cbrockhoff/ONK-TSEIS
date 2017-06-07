using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Broker.Persistence.Models;

namespace Broker.Persistence
{
    public interface IBuyOfferRepository
    {
        Task Write(Guid buyerId, string stockName, int amount, decimal price);
        Task<IEnumerable<BuyOfferReadModel>> Read();
        Task Delete(Guid buyerId, string stockName, int amount, decimal price);
    }
}