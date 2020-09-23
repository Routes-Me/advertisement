using System;

namespace AdvertisementService.Models.ResponseModel
{
    public class InstitutionsModel
    {
        public int InstitutionId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? PhoneNumber { get; set; }
        public string CountryIso { get; set; }
    }
}
