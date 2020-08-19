using AdvertisementService.Models;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IAdvertisementsRepository
    {
        AdvertisementsGetResponse GetAdvertisements(int advertisementId, string includeType, PageInfo pageInfo);
        AdvertisementsResponse UpdateAdvertisements(PostAdvertisementsModel model);
        AdvertisementsResponse DeleteAdvertisements(int id);
        AdvertisementsResponse InsertAdvertisements(PostAdvertisementsModel model);
    }
}
