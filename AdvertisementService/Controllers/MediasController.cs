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
    public class MediasController : BaseController
    {
        private readonly IMediasRepository _mediasRepository;
        public MediasController(IMediasRepository mediasRepository)
        {
            _mediasRepository = mediasRepository;
        }

        [HttpGet]
        [Route("medias/{id=0}")]
        public IActionResult Get(int id, string include, [FromQuery] PageInfo pageInfo)
        {
            MediasGetResponse response = new MediasGetResponse();
            response = _mediasRepository.GetMedias(id, include, pageInfo);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("medias")]
        public IActionResult Post([FromForm] MediasModel model)
        {
            MediasResponse response = new MediasResponse();
            response = _mediasRepository.InsertMedias(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("medias")]
        public IActionResult Put([FromForm] MediasModel model)
        {
            MediasResponse response = new MediasResponse();
            response = _mediasRepository.UpdateMedias(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("medias/{id}")]
        public IActionResult Delete(int id)
        {
            MediasResponse response = new MediasResponse();
            response = _mediasRepository.DeleteMedias(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }
    }
}
