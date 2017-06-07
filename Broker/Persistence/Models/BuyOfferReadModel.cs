using System;

namespace Broker.Persistence.Models
{
    public class BuyOfferReadModel
    {
        public Guid BuyerId { get; set; }
        public string Stock { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
    }
}