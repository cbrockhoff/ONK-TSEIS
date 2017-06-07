using System;

namespace Shared.Contracts.Events
{
    public class StockTradeHappenedEventDto : IEvent
    {
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public string Stock { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}