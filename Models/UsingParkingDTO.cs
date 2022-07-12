using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
   public class UsingParkingDTO
    {
        public long up_code;
        public long up_parkingCode;
        public DateTime up_date;
        public DateTime up_entranceHour;
        public DateTime up_leavingHour;
        public bool up_isUsing;
        public bool up_direction;
    }
}
