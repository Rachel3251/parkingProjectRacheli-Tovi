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
    [RoutePrefix("api/parkinglot")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]


    public class ParkingLotController : ApiController
    {
        BLL.ParkingLotBL parkingLotBL = new BLL.ParkingLotBL();

        [AcceptVerbs("GET", "POST")]

        [Route("add")]
        [HttpPost]
        public long Add(AddParkingLot addParking)
        {
            
            return parkingLotBL.AddParkingLot(addParking.parkingLot, addParking.blocks, addParking.parkings);
        }


        [Route("update")]
        [HttpPost]
        public long Update(ParkingLotDTO parkingLot)
        {
            return parkingLotBL.Update(parkingLot);
        }

        [Route("delete")]
        [HttpPost]
        public long Delete(ParkingLotDTO parkingLot)
        {
            return parkingLotBL.Delete(parkingLot);
        }

        [Route("getallparkinglots")]
        [HttpGet]
        public List<ParkingLotDTO> GetAllParkingLots()
        {
            return parkingLotBL.GetAllParkingLots();
        }

        //שליפת החניונים בעיר ע"פ קוד עיר
        [Route("getallparkinglotsincity/{citycode}")]
        [HttpGet]
        public List<ParkingLotDTO> GetAllParkingLotsInCity(long cityCode)
        {
            return parkingLotBL.GetAllParkingLotsInCity(cityCode);
        }

        [Route("getparkingLotbycode/{code}")]
        [HttpGet]
        public ParkingLotDTO GetParkingLotByCode(long code)
        {
            return parkingLotBL.GetParkingLotByCode(code);
        }

        //[Route("checkparkinglotfitting")]
        //[HttpPost]
        //public Queue<object> CheckParkingLotFitting(List<BlockDTO> blocks, Dictionary<long, ParkingDTO[][]> parkingsOfBlocks)
        //{
        //    return BuildParkingLotBL.CheckParkingLotFitting(blocks, parkingsOfBlocks);
        //}

        //[Route("getparkinglotbycity")]
        //[HttpGet]
        //public List<ParkingLotDTO> GetParkingLotbyCity(long code)
        //{
        //    return GetParkingLotbyCityBL(code);
        //}

        //מחזירה חניון ע"פ תעודת זהות של בעל חניון
        [Route("getparkinglotbyownerid")]
        [HttpGet]
        public long GetParkingLotByOwnerId(string id)
        {
            return parkingLotBL.GetParkingLotByOwnerId(id);
        }

        //מחזירה טקסט עם חוקי החניון
        [Route("getrules")]
        [HttpGet]
        public string GetRules()
        {
            return parkingLotBL.GetRules();
        }

        //מחזירה את מספר החניות שנכנסות באורך וברוחב
        [Route("numberofparkings")]
        [HttpGet]
        public int[] NumberOfParkings(double height, double width)
        {
            return parkingLotBL.NumberOfParkings(height, width);
        }

        //בדיקת תאימות בלוק (כל בלוק בנפרד!!) לפני שמירה
        [Route("checkparkinglotfitting")]
        [HttpPost]
        public bool CheckParkingLotFitting(List<ParkingDTO> list)
        {
            return parkingLotBL.CheckParkingLotFitting(list);
        }
        [Route("numberOfparkings/{h}/{w}")]
        [HttpGet]
        public int[] numberOfParkings(double h, double w)
        {
            return BuildParkingLotBL.NumberOfParkings(h, w);
        }
    }
}