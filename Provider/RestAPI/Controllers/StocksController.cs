using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Provider.RestAPI.Models;
using RestAPI.Helpers;
using Shared.Contracts.Commands;
using Shared.Messaging;

namespace Provider.RestAPI.Controllers
{
    [Route("api/[controller]")]
    public class StocksController : Controller
    {
        private readonly IMessageBus _bus;

        public StocksController(IMessageBus bus)
        {
            _bus = bus;
        }

        [HttpPut]
        [Route("sell")]
        public async Task<IActionResult> SellStock(SellStockInputModel input)
        {
            await _bus.Send(new SetStockForSaleCommandDto()
            {
                Stock = input.Name,
                Price = input.Price,
                Amount = input.Amount,
                SellerId = Request.GetAuthenticatedUser()
            });

            return Accepted();
        }
    }
}
