using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AdvertisementService.Controllers
{
    [Route("api")]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignsRepository _campaignsRepository;
        public CampaignsController(ICampaignsRepository campaignsRepository)
        {
            _campaignsRepository = campaignsRepository;
        }

        [HttpGet]
        [Route("campaigns/{id=0}")]
        public IActionResult GetCampaigns(string id, string include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _campaignsRepository.GetCampaigns(id, include, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpGet]
        [Route("campaigns/{id=0}/advertisements/{advertisementsId=0}")]
        public IActionResult GetAdvertisementsByIdAsync(string id, string advertisementsId, string include, string embed, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _campaignsRepository.GetAdvertisementsAsync(id, advertisementsId, include, embed, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("campaigns")]
        public IActionResult Post(CampaignsModel model)
        {
            dynamic response = _campaignsRepository.InsertCampaigns(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("campaigns")]
        public IActionResult Put(CampaignsModel model)
        {
            dynamic response = _campaignsRepository.UpdateCampaigns(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("campaigns/{id}")]
        public IActionResult Delete(string id)
        {
            dynamic response = _campaignsRepository.DeleteCampaigns(id);
            return StatusCode((int)response.statusCode, response);
        }
    }
}
