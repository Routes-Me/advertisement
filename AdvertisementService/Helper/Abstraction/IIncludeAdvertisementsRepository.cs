using AdvertisementService.Models.ResponseModel;
using System.Collections.Generic;

namespace AdvertisementService.Helper.Abstraction
{
    public interface IIncludeAdvertisementsRepository
    {
        dynamic GetInstitutionsIncludedData(List<AdvertisementsGetModel> advertisementsModelList);
        dynamic GetMediasIncludedData(List<AdvertisementsGetModel> advertisementsModelList);
        dynamic GetCampaignIncludedData(List<AdvertisementsGetModel> advertisementsModelList);
        dynamic GetIntervalIncludedData(List<AdvertisementsGetModel> advertisementsModelList);
        dynamic GetPromotionsIncludedData(List<ContentsModel> contentsModels);
        List<PromotionsGetModel> GetPromotionsData();
        dynamic GetPromotionsForAdvertisementIncludedData(List<AdvertisementsGetModel> advertisementsModelList);
    }
}
