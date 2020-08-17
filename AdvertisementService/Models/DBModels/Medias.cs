using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.DBModels
{
    public partial class Medias
    {
        public Medias()
        {
            Advertisements = new HashSet<Advertisements>();
        }

        public int MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaType { get; set; }
        public int? MediaMetadataId { get; set; }

        public virtual Mediametadata MediaMetadata { get; set; }
        public virtual ICollection<Advertisements> Advertisements { get; set; }
    }
}
