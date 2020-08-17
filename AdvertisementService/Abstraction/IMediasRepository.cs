using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IMediasRepository
    {
        MediasResponse GetMedias(GetMediasModel model);
        MediasResponse UpdateMedias(MediasModel model);
        MediasResponse DeleteMedias(int id);
        MediasResponse InsertMedias(MediasModel model);
    }
}
