using System.Collections.Generic;

namespace OwnerControl.RestAPI.Models
{
    public class UserStocksResultModel
    {
        public IEnumerable<UserStockResultModel> Stocks { get; set; }
    }

    public class UserStockResultModel
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }
}