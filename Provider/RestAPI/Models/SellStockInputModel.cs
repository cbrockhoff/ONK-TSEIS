using System;

namespace Provider.RestAPI.Models
{
    public class SellStockInputModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
    }
}