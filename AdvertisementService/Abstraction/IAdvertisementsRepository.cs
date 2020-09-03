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
        AdvertisementsGetResponse GetAdvertisements(int advertisementId, string includeType, Pagination pageInfo);
        AdvertisementsGetResponse GetAdvertisementsByInstitutionId(int id, int advertisementId, string includeType, Pagination pageInfo);
        AdvertisementsResponse UpdateAdvertisements(PostAdvertisementsModel model);
        AdvertisementsResponse DeleteAdvertisements(int id);
        AdvertisementsResponse InsertAdvertisements(PostAdvertisementsModel model);
    }
}
