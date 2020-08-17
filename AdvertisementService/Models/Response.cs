using AdvertisementService.Models.ResponseModel;
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
    public class CampaignsResponse : Response
    {
        public CampaignsDetails campaignsDetails { get; set; }
    }

    public class CampaignsDetails
    {
        public Pagination pagination { get; set; }
        public CampaignsDetailsData data { get; set; }
    }

    public class CampaignsDetailsData
    {
        public List<CampaignsModel> campaigns { get; set; }
        public List<AdvertisementsModel> advertisements { get; set; }
    }
    #endregion

    #region Advertisements Response
    public class AdvertisementsResponse : Response
    {
        public AdvertisementsDetails advertisementsDetails { get; set; }
    }

    public class AdvertisementsDetails
    {
        public Pagination pagination { get; set; }
        public AdvertisementsDetailsData data { get; set; }
    }

    public class AdvertisementsDetailsData
    {
        public List<AdvertisementsModel> advertisements { get; set; }
    }
    #endregion

    #region Medias Response
    public class MediasResponse : Response
    {
        public MediasDetails mediasDetails { get; set; }
    }

    public class MediasDetails
    {
        public Pagination pagination { get; set; }
        public MediasDetailsData data { get; set; }
    }

    public class MediasDetailsData
    {
        public List<MediasModel> medias { get; set; }
    }
    #endregion

    #region Intervals Response
    public class IntervalsResponse : Response
    {
        public IntervalsDetails intervalsDetails { get; set; }
    }

    public class IntervalsDetails
    {
        public Pagination pagination { get; set; }
        public IntervalsDetailsData data { get; set; }
    }

    public class IntervalsDetailsData
    {
        public List<IntervalsModel> intervals { get; set; }
    }
    #endregion
}