using System;

namespace Shared.Contracts.Commands
{
    public class SetStockForSaleCommandDto : ICommand
    {
        public Guid SellerId { get; set; }
        public string Stock { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}