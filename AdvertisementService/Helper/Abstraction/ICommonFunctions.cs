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
        List<AdvertisementsGetModelWithCampaign> GetAllAdvertisements(List<Advertisements> advertisements, List<AdvertisementsIntervals> BroadcastsData, Pagination pageInfo);
        List<AdvertisementsGetModel> GetAdvertisementWithCampaigns(List<AdvertisementsGetModelWithCampaign> advertisementsModelListWithCampaign);
  
    }
}
