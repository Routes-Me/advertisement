using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdvertisementService.Internal.Abstraction;
using AdvertisementService.Models;

namespace AdvertisementService.Internal.Controllers
{
    [ApiController]
    [ApiVersion( "1.0" )]
    [Route("v{version:apiVersion}/")]
    public class AdverisementsReportController : ControllerBase
    {
        private readonly IAdvertisementsReportRepository _advertisementsReportRepository;

        public AdverisementsReportController(IAdvertisementsReportRepository advertisementsReportRepository)
        {
            _advertisementsReportRepository = advertisementsReportRepository;
        }

        [HttpPost]
        [Route("advertisements/reports")]
        public IActionResult ReportAdvertisements(List<int> advertisementIds, [FromQuery] List<string> attr)
        {
            AdvertisementsGetReportDto institutionsGetReportDto = new AdvertisementsGetReportDto();
            try
            {
                institutionsGetReportDto.Data = _advertisementsReportRepository.ReportAdvertisements(advertisementIds, attr);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorMessage { Error = ex.Message });
            }
            return Ok(institutionsGetReportDto);
        }
    }
}
