using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementService.Controllers
{
    [Route("api")]
    [ApiController]
    public class CampaignsController : BaseController
    {
        private readonly ICampaignsRepository _campaignsRepository;
        public CampaignsController(ICampaignsRepository campaignsRepository)
        {
            _campaignsRepository = campaignsRepository;
        }

        [HttpGet]
        [Route("campaigns/{id=0}")]
        public IActionResult GetCampaigns(int id, int currentPage = 1, int pageSize = 10)
        {
            CampaignsResponse response = new CampaignsResponse();
            GetCampaignsModel Model = new GetCampaignsModel();
            Model.CampaignId = id;
            Model.isCampaign = true;
            Model.currentPage = currentPage;
            Model.pageSize = pageSize;
            response = _campaignsRepository.GetCampaigns(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpGet]
        [Route("campaigns/{id=0}/advertisements/{advertisementsId=0}")]
        public IActionResult GetCampaignsById(int id, int advertisementsId, int currentPage = 1, int pageSize = 10)
        {
            CampaignsResponse response = new CampaignsResponse();
            GetCampaignsModel Model = new GetCampaignsModel();
            Model.CampaignId = id;
            Model.isAdvertisement = true;
            Model.currentPage = currentPage;
            Model.pageSize = pageSize;
            response = _campaignsRepository.GetCampaigns(Model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("campaigns")]
        public IActionResult Post(CampaignsModel model)
        {
            CampaignsResponse response = new CampaignsResponse();
            if (ModelState.IsValid)
                response = _campaignsRepository.InsertCampaigns(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("campaigns")]
        public IActionResult Put(CampaignsModel model)
        {
            CampaignsResponse response = new CampaignsResponse();
            if (ModelState.IsValid)
                response = _campaignsRepository.UpdateCampaigns(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("campaigns/{id}")]
        public IActionResult Delete(int id)
        {
            CampaignsResponse response = new CampaignsResponse();
            if (ModelState.IsValid)
                response = _campaignsRepository.DeleteCampaigns(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
