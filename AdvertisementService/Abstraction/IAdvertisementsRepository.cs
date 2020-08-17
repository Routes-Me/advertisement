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
        AdvertisementsResponse GetAdvertisements(GetAdvertisementsModel model);
        AdvertisementsResponse UpdateAdvertisements(AdvertisementsModel model);
        AdvertisementsResponse DeleteAdvertisements(int id);
        AdvertisementsResponse InsertAdvertisements(AdvertisementsModel model);
    }
}
