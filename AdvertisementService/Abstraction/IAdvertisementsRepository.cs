using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;

namespace AdvertisementService.Abstraction
{
    public interface IAdvertisementsRepository
    {
        dynamic GetAdvertisements(string institutionId, string advertisementId, string includeType, Pagination pageInfo);
        dynamic UpdateAdvertisements(PostAdvertisementsModel model);
        dynamic DeleteAdvertisements(string id);
        dynamic InsertAdvertisements(PostAdvertisementsModel model);
        dynamic GetContents(string advertisementsId, Pagination pageInfo, string token);
    }
}
