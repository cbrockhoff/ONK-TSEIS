using System;

namespace Broker.Persistence.Models
{
    public class StockOwnerReadModel
    {
        private Guid OwnerId { get; set; }
        private string Stock { get; set; }
        private int Amount { get; set; }
    }
}