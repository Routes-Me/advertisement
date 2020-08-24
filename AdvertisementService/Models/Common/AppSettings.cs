using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.Common
{
    public class AppSettings
    {
        public string UserEndpointUrl { get; set; }
        public string InstitutionEndpointUrl { get; set; }
        public string QRCodeEndpointUrl { get; set; }
    }
}
