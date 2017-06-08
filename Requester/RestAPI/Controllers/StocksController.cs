using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Requester.Persistence;
using Requester.RestAPI.Models;
using RestAPI.Helpers;
using Shared.Contracts.Commands;
using Shared.Messaging;

namespace Requester.RestAPI.Controllers
{
    [Route("api/[controller]")]
    public class StocksController : Controller
    {
        private readonly IMessageBus _bus;
        private readonly IStocksForSaleReadRepository _forSaleReadRepository;

        public StocksController(IMessageBus bus, IStocksForSaleReadRepository forSaleReadRepository)
        {
            _bus = bus;
            _forSaleReadRepository = forSaleReadRepository;
        }

        [HttpGet]
        [Route("forsale")]
        public async Task<IActionResult> GetStocksForSale()
        {
            var forsale = await _forSaleReadRepository.Read();
            var result = new StocksForSaleResultModel()
            {
                Stocks = forsale.Select(rm => new StockForSaleResultModel()
                {
                    Name = rm.Stock,
                    Amount = rm.Amount,
                    Price = rm.Price
                })
            };
            return Ok(result);
        }

        [HttpPut]
        [Route("forsale/buy")]
        public async Task<IActionResult> BuyStocks(BuyStocksInputModel input)
        {
            await _bus.Send(new PlaceBuyOfferCommandDto()
            {
                BuyerId = Request.GetAuthenticatedUser(),
                Amount = input.Amount,
                Price = input.Price,
                Stock = input.Name
            }).ConfigureAwait(false);

            return Accepted();
        }
    }
}
