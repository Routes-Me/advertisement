using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementService.Controllers
{
    [Route("api")]
    [ApiController]
    public class AdvertisementsController : BaseController
    {
        private readonly IAdvertisementsRepository _advertisementsRepository;
        public AdvertisementsController(IAdvertisementsRepository advertisementsRepository)
        {
            _advertisementsRepository = advertisementsRepository;
        }

        [HttpGet]
        [Route("advertisements/{id=0}")]
        public IActionResult Get(int id, int currentPage = 1, int pageSize = 10)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            GetAdvertisementsModel model = new GetAdvertisementsModel();
            model.AdvertisementId = id;
            model.currentPage = currentPage;
            model.pageSize = pageSize;
            response = _advertisementsRepository.GetAdvertisements(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("advertisements")]
        public IActionResult Post(AdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            if (ModelState.IsValid)
                response = _advertisementsRepository.InsertAdvertisements(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("advertisements")]
        public IActionResult Put(AdvertisementsModel model)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            if (ModelState.IsValid)
                response = _advertisementsRepository.UpdateAdvertisements(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("advertisements/{id}")]
        public IActionResult Delete(int id)
        {
            AdvertisementsResponse response = new AdvertisementsResponse();
            if (ModelState.IsValid)
                response = _advertisementsRepository.DeleteAdvertisements(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
