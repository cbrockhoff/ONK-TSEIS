using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Test.Service.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> EchoValue(int id)
        {
            await Task.Delay(new Random().Next(20, 1000)); // heavy work
            return new ObjectResult(id);
        }
    }
}
