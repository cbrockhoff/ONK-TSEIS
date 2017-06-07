namespace Requester.RestAPI.Models
{
    public class BuyStocksInputModel
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}