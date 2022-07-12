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
    [RoutePrefix("api/city")]


    public class CityController : ApiController
    {
            BLL.CityBL cityBL = new BLL.CityBL();

        [AcceptVerbs("GET", "POST")]

        [Route("add")]
        [HttpPost]
        public long Add(CityDTO city)
        {
            return cityBL.Insert(city);
        }

        [Route("update")]
        [HttpPost]
        public long Update(CityDTO city)
        {
            return cityBL.Update(city);
        }

        [Route("delete")]
        [HttpPost]
        public long Delete(CityDTO city)
        {
            return cityBL.Delete(city);
        }

        [Route("getallcities")]
        [HttpGet]
        public List<CityDTO> GetAllCities()
        {
            return cityBL.GetAllCities();
        }

        [Route("getcitynamebyCode")]
        [HttpGet]
        public string GetCityByCode(long code)
        {
            return cityBL.GetCityNameByCode(code);
        }
    }
}