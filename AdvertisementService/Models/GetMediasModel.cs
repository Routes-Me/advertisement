using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models
{
    public class GetMediasModel
    {
        public int MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaType { get; set; }
        public float? Size { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float? Duration { get; set; } = 0;
    }
}
