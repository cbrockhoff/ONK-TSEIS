using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OwnerControl.Persistence;
using OwnerControl.RestAPI.Models;
using RestAPI.Helpers;

namespace OwnerControl.RestAPI.Controllers
{
    [Route("api/[controller]")]
    [RequireAuthenticatedUser]
    public class UsersController : Controller
    {
        private readonly IStocksRepository _stocksRepository;

        public UsersController(IStocksRepository stocksRepository)
        {
            _stocksRepository = stocksRepository;
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
    }
}