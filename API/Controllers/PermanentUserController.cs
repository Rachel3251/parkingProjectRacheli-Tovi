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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/permanentuser")]
    public class PermanentUserController:ApiController
    {
        BLL.PermanentUserBL permanentUserBL = new BLL.PermanentUserBL();

        //[AcceptVerbs("GET", "POST")]

        [Route("add")]
        [HttpPost]
        public long Add([FromBody] PermanentUserDTO permanentUser)
        {
            return permanentUserBL.Insert(permanentUser);
        }

        [Route("update")]
        [HttpPost]
        public long Update(PermanentUserDTO permanentUser)
        {
            return permanentUserBL.Update(permanentUser);
        }

        [Route("delete")]
        [HttpPost]
        public long Delete(PermanentUserDTO permanentUser)
        {
            return permanentUserBL.Delete(permanentUser);
        }

        [Route("getallpermanentusers")]
        [HttpGet]
        public List<PermanentUserDTO> GetAllPermanentUsers()
        {
            return permanentUserBL.GetAllPermanentUsers();
        }

        [Route("getpermanentusersbycode")]
        [HttpGet]
        public PermanentUserDTO GetPermanentUsersByCode(long code)
        {
            return permanentUserBL.GetPermanentUsersByCode(code);
        }

        //מחזירה את כל הימים האקטואליים בהם חונה קבוע רשום בחניון מסוים
        [Route("getallactualdaysinparkinglotbyuserid")]
        [HttpGet]
        public List<PermanentUserDTO> GetAllActualDaysInParkingLotByUserId(string id, long parkingLotCode)
        {
            return permanentUserBL.GetAllActualDaysInParkingLotByUserId(id, parkingLotCode);
        }

        //מחזירה את שעת הכניסה של החונה הקבוע היום
        [Route("getenterancehourofpermanentusertoday")]
        [HttpGet]
        public DateTime GetEnteranceHourOfPermanentUserToday(string id, long parkingLotCode)
        {
            return permanentUserBL.GetEnteranceHourOfPermanentUserToday(id, parkingLotCode);
        }

        //מחזירה את שעת היציאה של החונה הקבוע היום
        [Route("getleavinghourofpermanentusertoday")]
        [HttpGet]
        public DateTime GetLeavingHourOfPermanentUserToday(string id, long parkingLotCode)
        {
            return permanentUserBL.GetLeavingHourOfPermanentUserToday(id, parkingLotCode);
        }

        //מחזירה את קוד החניון של החונה הקבוע היום
        [Route("getparkinglotcodeofpermanentusertoday")]
        [HttpGet]
        public long GetParkingLotCodeOfPermanentUserToday(string id)
        {
            return permanentUserBL.GetParkingLotCodeOfPermanentUserToday(id);
        }
    }
}