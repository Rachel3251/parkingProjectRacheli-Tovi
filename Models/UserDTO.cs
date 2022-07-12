using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UserDTO
    {
        public long u_code;
        public string u_id;
        public string u_firstName;
        public string u_lastName;
        public int u_permission;
        public string u_password;
        public long u_parkingLotCode;
        public long u_email;

        public UserDTO()
        {

        }

        //public UserDTO(long code, string id, string firstName, string lastName, int permission, string password, long parkingLotCode)
        //{
        //    this.u_code = code;
        //    this.u_id = id;
        //    this.u_firstName = firstName;
        //    this.u_lastName = lastName;
        //    this.u_permission = permission;
        //    this.u_password = password;
        //    this.u_parkingLotCode = parkingLotCode;
        //}
    }
}
