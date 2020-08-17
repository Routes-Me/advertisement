using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.ResponseModel;
using Microsoft.CodeAnalysis;
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
                    response.mediasDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var advertisementData = _context.Advertisements.Where(x => x.MediaId == id).FirstOrDefault();
                if (advertisementData != null)
                {
                    response.status = false;
                    response.message = "Media is associated with other advertisemnts.";
                    response.mediasDetails = null;
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
                response.mediasDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while deleting Media. Error Message - " + ex.Message;
                response.mediasDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }

        public MediasResponse GetMedias(GetMediasModel model)
        {
            MediasResponse Response = new MediasResponse();
            MediasDetails objMediasDetails = new MediasDetails();
            int totalCount = 0;
            try
            {
                List<MediasModel> objMediasModelList = new List<MediasModel>();
                MediasDetailsData objMediasDetailsData = new MediasDetailsData();

                if (model.MediaId == 0)
                {
                    objMediasModelList = (from media in _context.Medias
                                          join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                          select new MediasModel()
                                          {
                                              MediaId = media.MediaId,
                                              CreatedAt = media.CreatedAt,
                                              Url = media.Url,
                                              MediaType = media.MediaType,
                                              MediaMetadataId = metadata.MediaMetadataId,
                                              Duration = metadata.Duration,
                                              Size = metadata.Size
                                          }).OrderBy(a => a.MediaId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  select new MediasModel()
                                  {
                                      MediaId = media.MediaId,
                                      CreatedAt = media.CreatedAt,
                                      Url = media.Url,
                                      MediaType = media.MediaType,
                                      MediaMetadataId = metadata.MediaMetadataId,
                                      Duration = metadata.Duration,
                                      Size = metadata.Size
                                  }).ToList().Count();
                }
                else
                {
                    objMediasModelList = (from media in _context.Medias
                                          join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                          where media.MediaId == model.MediaId
                                          select new MediasModel()
                                          {
                                              MediaId = media.MediaId,
                                              CreatedAt = media.CreatedAt,
                                              Url = media.Url,
                                              MediaType = media.MediaType,
                                              MediaMetadataId = metadata.MediaMetadataId,
                                              Duration = metadata.Duration,
                                              Size = metadata.Size
                                          }).OrderBy(a => a.MediaId).Skip((model.currentPage - 1) * model.pageSize).Take(model.pageSize).ToList();

                    totalCount = (from media in _context.Medias
                                  join metadata in _context.Mediametadata on media.MediaMetadataId equals metadata.MediaMetadataId
                                  where media.MediaId == model.MediaId
                                  select new MediasModel()
                                  {
                                      MediaId = media.MediaId,
                                      CreatedAt = media.CreatedAt,
                                      Url = media.Url,
                                      MediaType = media.MediaType,
                                      MediaMetadataId = metadata.MediaMetadataId,
                                      Duration = metadata.Duration,
                                      Size = metadata.Size
                                  }).ToList().Count();
                }

                if (objMediasModelList == null || objMediasModelList.Count == 0)
                {
                    Response.status = false;
                    Response.message = "Media not found.";
                    Response.mediasDetails = null;
                    Response.responseCode = ResponseCode.NotFound;
                    return Response;
                }

                var page = new Pagination
                {
                    TotalCount = totalCount,
                    CurrentPage = model.currentPage,
                    PageSize = model.pageSize,
                    TotalPages = (int)Math.Ceiling(decimal.Divide(totalCount, model.pageSize)),
                    IndexOne = ((model.currentPage - 1) * model.pageSize + 1),
                    IndexTwo = (((model.currentPage - 1) * model.pageSize + model.pageSize) <= totalCount ? ((model.currentPage - 1) * model.pageSize + model.pageSize) : totalCount)
                };

                objMediasDetailsData.medias = objMediasModelList;
                objMediasDetails.data = objMediasDetailsData;
                objMediasDetails.pagination = page;

                Response.message = "Media data retrived successfully.";
                Response.status = true;
                Response.mediasDetails = objMediasDetails;
                Response.responseCode = ResponseCode.Success;
                return Response;
            }
            catch (Exception ex)
            {
                Response.status = false;
                Response.message = "Something went wrong while fetching data. Error Message - " + ex.Message;
                Response.mediasDetails = null;
                Response.responseCode = ResponseCode.InternalServerError;
                return Response;
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
                    response.mediasDetails = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                Mediametadata objMediaMetadata = new Mediametadata()
                {
                    Duration = model.Duration,
                    Size = model.Size
                };
                _context.Mediametadata.Add(objMediaMetadata);
                _context.SaveChanges();

                Medias objMedia = new Medias()
                {
                    Url = model.Url,
                    CreatedAt = model.CreatedAt,
                    MediaType = model.MediaType,
                    MediaMetadataId = objMediaMetadata.MediaMetadataId
                };
                _context.Medias.Add(objMedia);
                _context.SaveChanges();

                response.status = true;
                response.message = "Media inserted successfully.";
                response.mediasDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while inserting Media. Error Message - " + ex.Message;
                response.mediasDetails = null;
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
                    response.mediasDetails = null;
                    response.responseCode = ResponseCode.BadRequest;
                    return response;
                }

                var mediaData = _context.Medias.Where(x => x.MediaId == model.MediaId).FirstOrDefault();
                if (mediaData == null)
                {
                    response.status = false;
                    response.message = "Media not found.";
                    response.mediasDetails = null;
                    response.responseCode = ResponseCode.NotFound;
                    return response;
                }

                var objMetadata = _context.Mediametadata.Where(x => x.MediaMetadataId == mediaData.MediaMetadataId).FirstOrDefault();
                if (objMetadata == null)
                {
                    Mediametadata mediaMetadata = new Mediametadata()
                    {
                        Duration = model.Duration,
                        Size = model.Size
                    };
                    _context.Mediametadata.Add(mediaMetadata);
                    mediaData.MediaMetadataId = mediaMetadata.MediaMetadataId;
                }
                else
                {
                    objMetadata.Duration = model.Duration;
                    objMetadata.Size = model.Size;
                    _context.Mediametadata.Update(objMetadata);
                }

                mediaData.Url = model.Url;
                mediaData.CreatedAt = model.CreatedAt;
                mediaData.MediaType = model.MediaType;
                _context.Medias.Update(mediaData);
                _context.SaveChanges();

                response.status = true;
                response.message = "Media updated successfully.";
                response.mediasDetails = null;
                response.responseCode = ResponseCode.Success;
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Something went wrong while updating Media. Error Message - " + ex.Message;
                response.mediasDetails = null;
                response.responseCode = ResponseCode.InternalServerError;
                return response;
            }
        }
    }
}
