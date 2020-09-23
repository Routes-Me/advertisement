using AdvertisementService.Models.ResponseModel;
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
            response.message = CommonMessage.ExceptionMessage + ex.Message;
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
            response.status = true;
            response.message = message;
            response.statusCode = statusCode;
            return response;
        }
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
        public List<AdvertisementsModel> data { get; set; }
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
        public int AdvertisementId { get; set; }
    }

    public class ActiveCampAdWithQRGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<GetActiveCampAdWithQRCodeModel> data { get; set; }
    }

    public class GetQrcodesModel
    {
        public int? AdvertisementId { get; set; }
        public string Details { get; set; }
        public string ImageUrl { get; set; }
    }
    #endregion

    #region QRCode Response
    public class GetQRCodeResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<GetQrcodesModel> data { get; set; }
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
        public int mediaId { get; set; }
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
        public List<PromotionsModel> data { get; set; }
    }
}