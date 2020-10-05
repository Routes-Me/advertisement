using AdvertisementService.Models.ResponseModel;
using System.Collections.Generic;

namespace AdvertisementService.Helper.Abstraction
{
    public interface IIncludeAdvertisementsRepository
    {
        dynamic GetInstitutionsIncludedData(List<AdvertisementsModel> advertisementsModelList);
        dynamic GetMediasIncludedData(List<AdvertisementsModel> advertisementsModelList);
        dynamic GetCampaignIncludedData(List<AdvertisementsModel> advertisementsModelList);
        dynamic GetIntervalIncludedData(List<AdvertisementsModel> advertisementsModelList);
        dynamic GetPromotionsIncludedData(List<AdvertisementsForContentModel> advertisementsModelList, string token);
    }
}
