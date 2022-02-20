using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IAdvertisementsRepository
    {
        dynamic GetAdvertisements (string institutionId, string advertisementId, string includeType, string embed, string sort_by, Pagination pageInfo);
        Task<dynamic> UpdateAdvertisementsAsync(PostAdvertisementsModel model);
        Task<dynamic> DeleteAdvertisementsAsync(string id);
        Task<dynamic> InsertAdvertisementsAsync(PostAdvertisementsModel model);
        dynamic GetContents(string advertisementsId, Pagination pageInfo,string institutionId);
        dynamic UpdateCampaignAdvertisement(string campaignsId, string advertisementsId, PatchSort model);
        dynamic UpdateCampaignAdvertisementList(string campaignsId, PatchSortList model);
    }
}
