namespace Shared.Contracts.Events
{
    public class StockSetForSaleEventDto : IEvent
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}