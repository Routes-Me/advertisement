using System.Threading.Tasks;
using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementService.Controllers
{
    [Route("api")]
    [ApiController]
    public class MediasController : ControllerBase
    {
        private readonly IMediasRepository _mediasRepository;
        public MediasController(IMediasRepository mediasRepository)
        {
            _mediasRepository = mediasRepository;
        }

        [HttpGet]
        [Route("medias/{id?}")]
        public IActionResult Get(string id, string include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _mediasRepository.GetMedias(id, include, pageInfo);
            return StatusCode(response.statusCode, response);
        }
        
        [HttpPost]
        [Route("medias")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Post([FromForm] MediasModel model)
        {
            dynamic response = await _mediasRepository.InsertMedias(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("medias")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Put([FromForm] MediasModel model)
        {
            dynamic response = await _mediasRepository.UpdateMedias(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("medias/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            dynamic response = await _mediasRepository.DeleteMedias(id);
            return StatusCode((int)response.statusCode, response);
        }
    }
}