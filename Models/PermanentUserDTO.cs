using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class PermanentUserDTO
    {
        public long cu_code;
        public long cu_userCode;
        public int cu_dayCode;
        public long cu_parkingLotCode;
        public DateTime cu_entranceHour;
        public DateTime cu_leavingHour;
        public DateTime cu_startDate;
        public DateTime cu_lastDate;
        public bool cu_status;
    }
}
