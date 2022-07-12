using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ParkingLotDTO
    {
        public long pl_code;
        public string pl_name;
        public string pl_location;
        public long pl_cityCode;
        public long pl_ownerCode;
        public long pl_numberBlocks;
        public DateTime pl_closeHour;
        public DateTime pl_openHour;
    }
}
