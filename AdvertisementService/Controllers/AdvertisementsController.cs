using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AdvertisementService.Controllers
{
    [Route("api")]
    [ApiController]
    public class AdvertisementsController : ControllerBase
    {
        private readonly IAdvertisementsRepository _advertisementsRepository;
        public AdvertisementsController(IAdvertisementsRepository advertisementsRepository)
        {
            _advertisementsRepository = advertisementsRepository;
        }

        [HttpGet]
        [Route("advertisements/{advertisementsId=0}")]
        public IActionResult Get(string advertisementsId, string include, [FromQuery] Pagination pageInfo)
        {
            string institutionId = "0";
            dynamic response = _advertisementsRepository.GetAdvertisements(institutionId, advertisementsId, include, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPost]
        [Route("advertisements")]
        public IActionResult Post(PostAdvertisementsModel model)
        {
            dynamic response = _advertisementsRepository.InsertAdvertisements(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPut]
        [Route("advertisements")]
        public IActionResult Put(PostAdvertisementsModel model)
        {
            dynamic response = _advertisementsRepository.UpdateAdvertisements(model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpDelete]
        [Route("advertisements/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            dynamic response = await _advertisementsRepository.DeleteAdvertisementsAsync(id);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpGet]
        [Route("institutions/{institutionId}/advertisements/{advertisementsId=0}")]
        public IActionResult GetAdvertisementsByInstitutionsId(string institutionId, string advertisementsId, string include, [FromQuery] Pagination pageInfo)
        {
            if (Convert.ToInt32(institutionId) <= 0)
            {
                dynamic resp = ReturnResponse.ErrorResponse(CommonMessage.InstitutionNotFound, StatusCodes.Status404NotFound);
                return StatusCode((int)resp.statusCode, resp);
            }
            dynamic response = _advertisementsRepository.GetAdvertisements(institutionId, advertisementsId, include, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpGet]
        [Route("contents/{id=0}")]
        public IActionResult GetAdvertisementsByPromotions(string id, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _advertisementsRepository.GetContents(id, pageInfo);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPatch]
        [Route("campaigns/{campaignsId}/advertisements/{advertisementsId}")]
        public IActionResult UpdateCampaignAdvertisement(string campaignsId, string advertisementsId, PatchSort model)
        {
            dynamic response = _advertisementsRepository.UpdateCampaignAdvertisement(campaignsId, advertisementsId, model);
            return StatusCode((int)response.statusCode, response);
        }

        [HttpPatch]
        [Route("campaigns/{campaignsId}/advertisements")]
        public IActionResult UpdateCampaignAdvertisementList(string campaignsId, PatchSortList model)
        {
            dynamic response = _advertisementsRepository.UpdateCampaignAdvertisementList(campaignsId, model);
            return StatusCode((int)response.statusCode, response);
        }
    }
}
