using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ParkingDTO
    {
        public long p_code;
        public string p_name;
        public long p_parkingLotCode;
        public long p_Location_i;
        public long p_Location_j;
        public bool p_isLegal; //מיותר?
        public string p_status; // PARKING / ENTERANCE / EXIT / PASS / ILLEGAL
        public long p_blockNumber;

    }
}
