using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AdvertisementService.Controllers
{
    [Route("api")]
    [ApiController]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignsRepository _campaignsRepository;
        private readonly advertisementserviceContext _context;
        public CampaignsController(ICampaignsRepository campaignsRepository, advertisementserviceContext context)
        {
            _campaignsRepository = campaignsRepository;
            _context = context;
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
        public IActionResult GetAdvertisementsByIdAsync(string id, string advertisementsId, string include, string embed,string sort_by, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _campaignsRepository.GetAdvertisementsAsync(id, advertisementsId, include, embed, sort_by, pageInfo);
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

        [HttpPost]
        [Route("campaigns/{campaignId}/broadcasts")]
        public async Task<IActionResult> CreateBroadcasts(string campaignId, BroadcastsDto broadcastsDto)
        {
            Broadcasts broadcast= new Broadcasts();
            try
            {
                broadcast = _campaignsRepository.CreateBroadcasts(campaignId, broadcastsDto);
                await _context.Broadcasts.AddAsync(broadcast);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            return StatusCode(StatusCodes.Status201Created, broadcast);
        }
    }
}
