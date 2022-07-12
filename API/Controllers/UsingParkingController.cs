using BLL;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API.Controllers
{
    [RoutePrefix("api/usingparking")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UsingParkingController
    {

        UsingParkingBL usingParkingBL = new BLL.UsingParkingBL();

        [AcceptVerbs("GET", "POST")]

        [Route("add")]
        [HttpPost]
        public long Add(UsingParkingDTO usingParking)
        {
            return usingParkingBL.Insert(usingParking);
        }

        [Route("update")]
        [HttpPost]
        public long Update(UsingParkingDTO usingParking)
        {
            return usingParkingBL.Update(usingParking);
        }

        [Route("delete")]
        [HttpPost]
        public long Delete(UsingParkingDTO usingParking)
        {
            return usingParkingBL.Delete(usingParking);
        }

        [Route("getallusingparking")]
        [HttpGet]
        public List<UsingParkingDTO> GetAllUsingParking()
        {
            return usingParkingBL.GetAllUsingParking();
        }

        [Route("getusingparkingbycode")]
        [HttpPost]
        //fix
        public UsingParkingDTO GetUsingParkingByCode(long code)
        {
            return usingParkingBL.GetUsingParkingByCode(code);
        }
    }
}