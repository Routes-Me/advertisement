using AdvertisementService.Helper.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.ResponseModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Net;

namespace AdvertisementService.Helper.Repository
{
    public class IncludeQRCodeRepository : IIncludeQRCodeRepository
    {
        private readonly AppSettings _appSettings;
        public IncludeQRCodeRepository(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public List<GetQrcodesModel> GetQRCodeIncludedData(List<GetActiveCampAdModel> advertisementsModel)
        {
            List<GetQrcodesModel> qrcodesDetails = new List<GetQrcodesModel>();
            foreach (var item in advertisementsModel)
            {
                var client = new RestClient(_appSettings.QRCodeEndpointUrl + item.ContentId);
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = response.Content;
                    var qrcodes = JsonConvert.DeserializeObject<GetQRCodeResponse>(result);
                    qrcodesDetails.AddRange(qrcodes.data);
                }
            }
            return qrcodesDetails;
        }
    }
}
