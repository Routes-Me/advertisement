using AdvertisementService.Models.ResponseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class Response
    {
        public bool status { get; set; }
        public string message { get; set; }
        public ResponseCode responseCode { get; set; }
    }
    public enum ResponseCode
    {
        Success = 200,
        Error = 2,
        InternalServerError = 500,
        MovedPermanently = 301,
        NotFound = 404,
        BadRequest = 400,
        Conflict = 409,
        Created = 201,
        NotAcceptable = 406,
        Unauthorized = 401,
        RequestTimeout = 408,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        Permissionserror = 403,
        Forbidden = 403,
        TokenRequired = 499,
        InvalidToken = 498
    }

    #region Campaigns Response
    public class CampaignsResponse : Response { }
    public class CampaignsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<CampaignsModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }
    #endregion

    #region Advertisements Response
    public class AdvertisementsResponse : Response { }
    public class AdvertisementsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<AdvertisementsModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
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
    public class MediasResponse : Response { }
    public class MediasGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public List<GetMediasModel> data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }
    #endregion

    #region Intervals Response

    public class IntervalsResponse : Response { }
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
}