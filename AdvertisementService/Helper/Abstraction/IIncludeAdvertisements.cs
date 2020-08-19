using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Abstraction
{
    public interface IIncludeAdvertisements
    {
        dynamic GetInstitutionsIncludedData(List<AdvertisementsModel> advertisementsModelList);
        dynamic GetMediasIncludedData(List<AdvertisementsModel> advertisementsModelList);
    }
}
