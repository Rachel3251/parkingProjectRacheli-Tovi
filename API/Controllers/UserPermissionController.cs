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
    [RoutePrefix("api/userpermission")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UserPermissionController
    {
        UserPermissionBL userPermissionBL = new UserPermissionBL();
  
        [AcceptVerbs("GET", "POST")]
        [Route("getalluserpermissions")]

        [HttpGet]
        public List<UserPermissionDTO> GetAllUserPermissions()
        {
            return userPermissionBL.GetAllUserPermissions();
        }

        [Route("getuserpermissionbyid")]
        [HttpPost]
        public UserPermissionDTO GetUserPermissionByCode(int code)
        {
            return userPermissionBL.GetUserPermissionByCode(code);
        }
    }
}