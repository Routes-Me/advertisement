using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.ResponseModel
{
    public class InstitutionsModel
    {
        public string InstitutionId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryIso { get; set; }
        public List<string> services { get; set; }
    }
}
