using System.Collections.Generic;

namespace Requester.RestAPI.Models
{
    public class StocksForSaleResultModel
    {
        public IEnumerable<StockForSaleResultModel> Stocks { get; set; }
    }

    public class StockForSaleResultModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
    }
}