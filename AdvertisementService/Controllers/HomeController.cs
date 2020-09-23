using Microsoft.AspNetCore.Mvc;

namespace AdvertisementService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Advertisement service started successfully";
        }
    }
}
