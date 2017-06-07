using System;

namespace OwnerControl.Persistence.Models
{
    public class UserStockReadModel
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
    }
}