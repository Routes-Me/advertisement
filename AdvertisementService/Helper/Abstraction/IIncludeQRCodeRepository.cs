using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Abstraction
{
    public interface IIncludeQRCodeRepository
    {
        List<GetQrcodesModel> GetQRCodeIncludedData(List<GetActiveCampAdModel> advertisementsModel);
    }
}
