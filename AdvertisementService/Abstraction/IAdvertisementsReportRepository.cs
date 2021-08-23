using System.Collections.Generic;
using AdvertisementService.Internal.Dto;

namespace AdvertisementService.Internal.Abstraction
{
    public interface IAdvertisementsReportRepository
    {
        List<AdvertisementReportDto> ReportAdvertisements(List<int> advertisementIds, List<string> attributes);
    }
}
