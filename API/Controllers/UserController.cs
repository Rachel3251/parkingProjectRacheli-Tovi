using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using BLL;
using Models;
using System.Web.Http.Cors;

namespace API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/user")]
    public class UserController : ApiController


    {
        UsersBL userBL = new UsersBL();

        [Route("signup")]
        [HttpPost]
        public long SignUp([FromBody] UserDTO user)
        {
            return userBL.Insert(user);
        }

        [Route("update")]
        [HttpPost]
        public long Update(UserDTO user)
        {
            return userBL.Update(user);
        }

        [Route("delete")]
        [HttpPost]
        public long Delete(UserDTO user)
        {
            return userBL.Delete(user);
        }

        [Route("getallusers")]
        [HttpGet]
        public List<UserDTO> GetAllUsers()
        {
            return userBL.GetAllUsers();
        }

        [Route("getuserbyid/{id}")]
        [HttpGet]
        public long GetUserById(string id)
        {
            return userBL.GetUserById(id);
        }

        //בדיקת תקינות תעודת זהות וסיסמה
        [Route("getuserbyidandpassword/{id}/{password}")]
        [HttpGet]
        public UserDTO GetUserByIdAndPassword(string id, string password)
        {
            return userBL.GetUserByIdAndPassword(id, password);
        }

    }
}