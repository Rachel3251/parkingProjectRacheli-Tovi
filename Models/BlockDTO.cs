using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class BlockDTO
    {
        public long b_code;
        public long b_parkingLotCode;
        public long b_numberParkingsForI;
        public long b_numberParkingsForJ;
        public long b_legalParkings;
        public Nullable<long> b_codeName;
        public string b_enteranceDirection;
        //public virtual ParkingLot ParkingLot { get; set; }
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<Parking> Parkings { get; set; }

    }
}
