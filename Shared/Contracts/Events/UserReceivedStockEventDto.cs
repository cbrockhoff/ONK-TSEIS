using System;

namespace Shared.Contracts.Events
{
    public class UserReceivedStockEventDto : IEvent
    {
        public Guid UserId { get; set; }
        public string Stock { get; set; }
        public int Amount { get; set; }
    }
}