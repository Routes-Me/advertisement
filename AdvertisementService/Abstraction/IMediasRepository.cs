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
        MediasGetResponse GetMedias(int mediaId, string include, Pagination pageInfo);
        Task<MediasResponse> UpdateMedias(MediasModel model);
        Task<MediasResponse> DeleteMedias(int id);
        Task<MediasResponse> InsertMedias(MediasModel model);
    }
}
