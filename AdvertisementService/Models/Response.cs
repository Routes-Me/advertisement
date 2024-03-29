﻿using AdvertisementService.Models.ResponseModel;
using AdvertisementService.Internal.Dto;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AdvertisementService.Models
{
    public class Response
    {
        public bool status { get; set; }
        public string message { get; set; }
        public int statusCode { get; set; }
    }
    public class ReturnResponse
    {
        public static dynamic ExceptionResponse(Exception ex)
        {
            Response response = new Response();
            response.status = false;
            response.message = CommonMessage.GenericException + ex.Message + "******** Stack Trace ***********" + ex.StackTrace;
            response.statusCode = StatusCodes.Status500InternalServerError;
            return response;
        }

        public static dynamic SuccessResponse(string message, bool isCreated)
        {
            Response response = new Response();
            response.status = true;
            response.message = message;
            if (isCreated)
                response.statusCode = StatusCodes.Status201Created;
            else
                response.statusCode = StatusCodes.Status200OK;
            return response;
        }

        public static dynamic ErrorResponse(string message, int statusCode)
        {
            Response response = new Response();
            response.status = false;
            response.message = message;
            response.statusCode = statusCode;
            return response;
        }
    }

    public class ErrorMessage
    {
        public string Error { get; set; }
    }

    #region Campaigns Response
    public class CampaignsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<CampaignsModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }
    #endregion

    #region Advertisements Response
    public class AdvertisementsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<AdvertisementsGetModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }

    public class ContentsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<ContentsModel> data { get; set; }
    }

    public class AdvertisementsPostResponse : Response
    {
        public string AdvertisementId { get; set; }
    }

    public class AdvertisementsGetReportDto
    {
        public List<AdvertisementReportDto> Data { get; set; }
    }

    #endregion

    #region Medias Response
    public class MediasGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<GetMediasModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }

    public class MediasInsertResponse : Response
    {
        public string mediaId { get; set; }
        public string url { get; set; }
    }
    #endregion

    #region Intervals Response

    public class IntervalsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<IntervalsModel> data { get; set; }
    }
    #endregion

    #region Institution Response
    public class InstitutionGetResponse : Response
    {
        public List<InstitutionsModel> data { get; set; }
    }
    #endregion
    public class PromotionsGetResponse : Response
    {
        public List<PromotionsGetModel> data { get; set; }
    }

    public class LinkResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<LinksModel> data { get; set; }
    }

    public class LinksModel
    {
        public string LinkId { get; set; }
        public string PromotionId { get; set; }
        public string Web { get; set; }
        public string Ios { get; set; }
        public string Android { get; set; }
    }

    public class CouponResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<CouponsModel> data { get; set; }
    }
    public class CouponsModel
    {
        public string CouponId { get; set; }
        public string PromotionId { get; set; }
        public string UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class VideoMetadata
    {
        public string CompressedFile { get; set; }
        public float Duration { get; set; }
        public float VideoSize { get; set; }
    }

    public class BroadcastsResponse
    {
        public string BroadcastId { get; set; }
    }

    public class ResourceNamesResponse
    {
        public string resourceName { get; set; }
    }
}