﻿using System;
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
    public class IntervalsController : BaseController
    {
        private readonly IIntervalsRepository _intervalsRepository;
        public IntervalsController(IIntervalsRepository intervalsRepository)
        {
            _intervalsRepository = intervalsRepository;
        }

        [HttpGet]
        [Route("intervals/{id=0}")]
        public IActionResult Get(int id, [FromQuery] PageInfo pageInfo)
        {
            IntervalsGetResponse response = new IntervalsGetResponse();
            response = _intervalsRepository.GetIntervals(id, pageInfo);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPost]
        [Route("intervals")]
        public IActionResult Post(IntervalsModel model)
        {
            IntervalsResponse response = new IntervalsResponse();
            response = _intervalsRepository.InsertIntervals(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpPut]
        [Route("intervals")]
        public IActionResult Put(IntervalsModel model)
        {
            IntervalsResponse response = new IntervalsResponse();
            response = _intervalsRepository.UpdateIntervals(model);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

        [HttpDelete]
        [Route("intervals/{id}")]
        public IActionResult Delete(int id)
        {
            IntervalsResponse response = new IntervalsResponse();
            response = _intervalsRepository.DeleteIntervals(id);
            if (response.responseCode != ResponseCode.Success)
                return GetActionResult(response);
            return Ok(response);
        }

    }
}
