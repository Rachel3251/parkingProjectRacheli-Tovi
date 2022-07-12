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
    [RoutePrefix("api/parking")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ParkingController : ApiController
    {
        ParkingBL parkingBL = new ParkingBL();

        [AcceptVerbs("GET", "POST")]

        //[Route("addallparkings")]
        //[HttpPost]
        //public long AddAllParkings(List<ParkingDTO> parkingsList)
        //{
        //    return parkingBL.InsertAllParkingsOfParkingLot(parkingsList);
        //}

        //[Route("add")]
        //[HttpPost]
        //public long Add(ParkingDTO parking)
        //{
        //    return parkingBL.Insert(parking);
        //}

        //[Route("update")]
        //[HttpPost]
        //public long Update(ParkingDTO parking)
        //{
        //    return parkingBL.Update(parking);
        //}

        //[Route("delete")]
        //[HttpPost]
        //public long Delete(ParkingDTO parking)
        //{
        //    return parkingBL.Delete(parking);
        //}

        [Route("getallparkings")]
        [HttpGet]
        public List<ParkingDTO> GetAllParkings()
        {
            return parkingBL.GetAllParkings();
        }

        [Route("getparkingbycode")]
        [HttpPost]
        public ParkingDTO GetParkingByCode(long code)
        {
            return parkingBL.GetParkingByCode(code);
        }

        //[Route("getusingparkingsbyparkinglotcode")]
        //[HttpPost]
        //public ParkingDTO getusingparkingsbyparkinglotcode(long code)
        //{
        //    return parkingBL.GetParkingByCode(code);
        //}


        //קבלת כל החניות של בלוק ע"פ קוד בלוק
        [Route("getallparkingsofblock")]
        [HttpGet]
        public List<ParkingDTO> GetAllParkingsOfBlock(long blockCode)
        {
            return parkingBL.GetAllParkingsOfBlock(blockCode);
        }
        [Route("findparking/")]
        [HttpPost]
        public ParkingDTO findParking([FromBody] Find find)
        {
            ParkingLotDTO parkingLot = find.parkingLot;
            UsingParkingDTO request = find.request;
            bool status = find.status;
            return FindParkingBL.FindParking(parkingLot,request,status);
        }
    }
}
