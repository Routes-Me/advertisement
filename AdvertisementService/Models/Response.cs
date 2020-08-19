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
        public CampaignsDetails data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }

    public class CampaignsDetails
    {
        public List<CampaignsModel> campaigns { get; set; }
    }
    #endregion

    #region Advertisements Response
    public class AdvertisementsResponse : Response { }
    public class AdvertisementsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public AdvertisementsDetails data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }

    public class AdvertisementsDetails
    {
        public List<AdvertisementsModel> advertisements { get; set; }
    }
    #endregion

    #region Medias Response
    public class MediasResponse : Response { }
    public class MediasGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public MediasDetails data { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject included { get; set; }
    }

    public class MediasDetails
    {
        public List<GetMediasModel> medias { get; set; }
    }
    #endregion

    #region Intervals Response

    public class IntervalsResponse : Response { }
    public class IntervalsGetResponse : Response
    {
        public Pagination pagination { get; set; }
        public IntervalsDetails data { get; set; }
    }

    public class IntervalsDetails
    {
        public List<IntervalsModel> intervals { get; set; }
    }
    #endregion

    #region Institution Response
    public class InstitutionGetResponse : Response
    {
        public InstitutionDetails data { get; set; }
    }

    public class InstitutionDetails
    {
        public List<InstitutionsModel> institution { get; set; }
    }
    #endregion
}