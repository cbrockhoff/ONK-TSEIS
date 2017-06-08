using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OwnerControl.Persistence;
using OwnerControl.RestAPI.Models;
using RestAPI.Helpers;
using Shared.Contracts.Events;
using Shared.Messaging;

namespace OwnerControl.RestAPI.Controllers
{
    [Route("api/[controller]")]
    [RequireAuthenticatedUser]
    public class UsersController : Controller
    {
        private readonly IMessageBus _bus;
        private readonly IStocksRepository _stocksRepository;

        public UsersController(IStocksRepository stocksRepository, IMessageBus bus)
        {
            _stocksRepository = stocksRepository;
            _bus = bus;
        }

        [HttpGet]
        [Route("me/stocks")]
        public async Task<IActionResult> GetStocks()
        {
            var stocks = await _stocksRepository.GetAll(Request.GetAuthenticatedUser());
            var result = new UserStocksResultModel()
            {
                Stocks = stocks.Select(rm => new UserStockResultModel()
                {
                    Name = rm.Name,
                    Amount = rm.Amount
                })
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("me/stocks")]
        public async Task<IActionResult> AddStocks(AddStocksInputModel input)
        {
            await _stocksRepository.Write(Request.GetAuthenticatedUser(), input.Name, input.Amount);
            await _bus.Publish(new UserReceivedStockEventDto()
            {
                UserId = Request.GetAuthenticatedUser(),
                Stock = input.Name,
                Amount = input.Amount
            });
            return Ok();
        }
    }
}