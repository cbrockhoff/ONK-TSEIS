using System;

namespace Requester.Persistence.Models
{
    public class StockForSaleReadModel
    {
        public Guid SellerId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
    }
}