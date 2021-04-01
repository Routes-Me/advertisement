using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class IntervalsVersionedController : ControllerBase
    {
        private readonly IIntervalsRepository _intervalsRepository;
        public IntervalsVersionedController(IIntervalsRepository intervalsRepository)
        {
            _intervalsRepository = intervalsRepository;
        }

        [HttpGet]
        [Route("intervals/{id=0}")]
        public IActionResult Get(string id, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _intervalsRepository.GetIntervals(id, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("intervals")]
        public IActionResult Post(IntervalsModel model)
        {
            dynamic response = _intervalsRepository.InsertIntervals(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("intervals")]
        public IActionResult Put(IntervalsModel model)
        {
            dynamic response = _intervalsRepository.UpdateIntervals(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("intervals/{id}")]
        public IActionResult Delete(string id)
        {
            dynamic response = _intervalsRepository.DeleteIntervals(id);
            return StatusCode((int)response.statusCode, response);
        }
    }
}