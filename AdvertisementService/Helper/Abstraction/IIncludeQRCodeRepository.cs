using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System.Collections.Generic;

namespace AdvertisementService.Helper.Abstraction
{
    public interface IIncludeQRCodeRepository
    {
        List<GetQrcodesModel> GetQRCodeIncludedData(List<GetActiveCampAdModel> advertisementsModel);
    }
}
