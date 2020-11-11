using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Helper.Abstraction
{
    public interface ICommonFunctions
    {
        List<AdvertisementsGetModel> GetAllAdvertisements(List<Advertisements> advertisements, List<AdvertisementsIntervals> advertisementsCampaignsData, Pagination pageInfo);
        List<AdvertisementsGetModel> GetAdvertisementWithCampaigns(List<AdvertisementsGetModel> advertisementsModelList);
  
    }
}
