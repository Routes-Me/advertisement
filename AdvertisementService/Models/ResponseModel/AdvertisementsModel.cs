﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.ResponseModel
{
    public class AdvertisementsModel
    {
        public int AdvertisementId { get; set; }
        public int? InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MediaId { get; set; }
    }
}
