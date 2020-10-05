using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Models.Common
{
    public class Random
    {
        public static string RandomUnique(int Size)
        {
            Guid myGuid = Guid.NewGuid();
            string UniqueNumber = myGuid.ToString("N");
            if (Size > 32)
            {
                Size = 32;
            }
            UniqueNumber = UniqueNumber.Substring(0, Size);
            return UniqueNumber.ToUpper();
        }
    }
}
