using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RoutesSecurity;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignsRepository _campaignsRepository;
        private readonly AdvertisementContext _context;
        private readonly AppSettings _appSettings;
        public CampaignsController(ICampaignsRepository campaignsRepository, AdvertisementContext context, IOptions<AppSettings> appSettings)
        {
            _campaignsRepository = campaignsRepository;
            _context = context;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        [Route("campaigns/{id?}")]
        public IActionResult GetCampaigns(string id, string include, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _campaignsRepository.GetCampaigns(id, include, pageInfo);
            return StatusCode(response.statusCode, response);
        }

        [HttpGet]
        [Route("campaigns/{id}/advertisements/{advertisementsId?}")]
        public IActionResult GetAdvertisementsByIdAsync(string id, string advertisementsId, string include, string embed,string sort_by, [FromQuery] Pagination pageInfo)
        {
            dynamic response = _campaignsRepository.GetAdvertisementsAsync(id, advertisementsId, include, embed, sort_by, pageInfo);
            return StatusCode(response.statusCode, response);
        }

        [HttpPost]
        [Route("campaigns")]
        public IActionResult Post(CampaignsModel model)
        {
            dynamic response = _campaignsRepository.InsertCampaigns(model);
            return StatusCode(response.statusCode, response);
        }

        [HttpPut]
        [Route("campaigns")]
        public IActionResult Put(CampaignsModel model)
        {
            dynamic response = _campaignsRepository.UpdateCampaigns(model);
            return StatusCode(response.statusCode, response);
        }

        [HttpDelete]
        [Route("campaigns/{id}")]
        public IActionResult Delete(string id)
        {
            dynamic response = _campaignsRepository.DeleteCampaigns(id);
            return StatusCode(response.statusCode, response);
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
            BroadcastsResponse broadcastsResponse = new BroadcastsResponse
            {
                BroadcastId =Obfuscation.Encode(broadcast.BroadcastId)
            };
            return StatusCode(StatusCodes.Status201Created, broadcastsResponse);
        }

        [HttpDelete]
        [Route("campaigns/{campaignId}/broadcasts/{broadcastId}")]
        public async Task<IActionResult> DeleteBroadcasts(string campaignId, string broadcastId)
        {
            Broadcasts broadcast= new Broadcasts();
            try
            {
                broadcast = _campaignsRepository.DeleteBroadcasts(campaignId, broadcastId);
                _context.Broadcasts.Remove(broadcast);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ex.Message);
            }
            catch (NullReferenceException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, CommonMessage.ExceptionMessage + ex.Message);
            }
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
