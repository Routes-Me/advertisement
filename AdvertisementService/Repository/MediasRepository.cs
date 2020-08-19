using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class MediasRepository : IMediasRepository
    {
        private readonly advertisementserviceContext _context;
        public MediasRepository(advertisementserviceContext context)
        {
            _context = context;
        }

        public MediasResponse DeleteMedias(int id)
        {
            MediasResponse response = new MediasResponse();
            try
            {
                var medias = _context.Medias.Where(x => x.MediaId == id).FirstOrDefault();
                if (medias == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementData = _context.Advertisements.Where(x => x.MediaId == id).FirstOrDefault();
                if (advertisementData != null)
                {
                    response.status = false;
                    response.message = "Media is associated with other advertisemnts.";
                    response.responseCode = ResponseCode.Conflict;
                    return response;
                }

                var metaData = _context.Mediametadata.Where(x => x.MediaMetadataId == medias.MediaMetadataId).FirstOrDefault();
                if (metaData != null)
                    _context.Mediametadata.Remove(metaData);

                _context.Medias.Remove(medias);
                _context.SaveChanges();
                response.status = true;
                response.message = "Media deleted successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Media. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public MediasGetResponse GetMedias(int mediaId, string includeType, PageInfo pageInfo)
        {
            MediasGetResponse response = new MediasGetResponse();
            MediasDetails mediasDetails = new MediasDetails();
            int totalCount = 0;
            try
            {
                List<GetMediasModel> mediasModelList = new List<GetMediasModel>();
                if (mediaId == 0)
                {
                    mediasModelList = (from media in _context.Medias
                                       join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                       select new GetMediasModel()
                                       {
                                           MediaId = media.MediaId,
                                           CreatedAt = media.CreatedAt,
                                           Url = media.Url,
                                           MediaType = media.MediaType,
                                           Duration = metadata.Duration,
                                           Size = metadata.Size
                                       }).OrderBy(a => a.MediaId).Skip((pageInfo.currentPage - 1) * pageInfo.pageSize).Take(pageInfo.pageSize).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  select new GetMediasModel()
                                  {
                                      MediaId = media.MediaId,
                                      CreatedAt = media.CreatedAt,
                                      Url = media.Url,
                                      MediaType = media.MediaType,
                                      Duration = metadata.Duration,
                                      Size = metadata.Size
                                  }).ToList().Count();
                }
                else
                {
                    mediasModelList = (from media in _context.Medias
                                       join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                       where media.MediaId == mediaId
                                       select new GetMediasModel()
                                       {
                                           MediaId = media.MediaId,
                                           CreatedAt = media.CreatedAt,
                                           Url = media.Url,
                                           MediaType = media.MediaType,
                                           Duration = metadata.Duration,
                                           Size = metadata.Size
                                       }).OrderBy(a => a.MediaId).Skip((pageInfo.currentPage - 1) * pageInfo.pageSize).Take(pageInfo.pageSize).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  where media.MediaId == mediaId
                                  select new GetMediasModel()
                                  {
                                      MediaId = media.MediaId,
                                      CreatedAt = media.CreatedAt,
                                      Url = media.Url,
                                      MediaType = media.MediaType,
                                      Duration = metadata.Duration,
                                      Size = metadata.Size
                                  }).ToList().Count();
                }

                if (mediasModelList == null || mediasModelList.Count == 0)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                mediasDetails.medias = mediasModelList;
                var page = new Pagination
                {
                    offset = pageInfo.currentPage,
                    limit = pageInfo.pageSize,
                    total = totalCount
                };

                response.status = true;
                response.message = "Media data retrived successfully.";
                response.pagination = page;
                response.data = mediasDetails;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while fetching medias. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public MediasResponse InsertMedias(MediasModel model)
        {
            MediasResponse response = new MediasResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                Mediametadata mediaMetadata = new Mediametadata()
                {
                    Duration = model.Duration,
                    Size = model.Size
                };
                _context.Mediametadata.Add(mediaMetadata);
                _context.SaveChanges();

                Medias objMedia = new Medias()
                {
                    Url = "http://localhost:56411/uploads/" + model.media.FileName,
                    //Url = "http://localhost:56411/uploads/" + model.Url,
                    CreatedAt = model.CreatedAt,
                    MediaType = model.MediaType,
                    MediaMetadataId = mediaMetadata.MediaMetadataId
                };
                _context.Medias.Add(objMedia);
                _context.SaveChanges();

                response.status = true;
                response.message = "Media inserted successfully.";
                response.responseCode = ResponseCode.Created;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Media. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public MediasResponse UpdateMedias(MediasModel model)
        {
            MediasResponse response = new MediasResponse();
            try
            {
                if (model == null)
                {
                    response.status = false;
                    response.message = "Pass valid data in model.";
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var mediaData = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (mediaData == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var metadata = _context.Mediametadata.Where(x => x.MediaMetadataId == mediaData.MediaMetadataId).FirstOrDefault();
                if (metadata == null)
                {
                    Mediametadata mediaMetadata = new Mediametadata()
                    {
                        Duration = model.Duration,
                        Size = model.Size
                    };
                    _context.Mediametadata.Add(mediaMetadata);
                    _context.SaveChanges();
                    mediaData.MediaMetadataId = mediaMetadata.MediaMetadataId;
                }
                else
                {
                    metadata.Duration = model.Duration;
                    metadata.Size = model.Size;
                    _context.Mediametadata.Update(metadata);
                    _context.SaveChanges();
                }

                mediaData.Url = model.Url;
                mediaData.CreatedAt = model.CreatedAt;
                mediaData.MediaType = model.MediaType;
                _context.Medias.Update(mediaData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Media updated successfully.";
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Media. Error Message - " + ex.Message;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
