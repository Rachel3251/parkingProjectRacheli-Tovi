using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using AutoMapper;
namespace BLL
{
    public class UsersBL
    {
      public static  List<UserDTO> UsersList = new List<UserDTO>();

        public UsersBL()
        {
            RefreshUsersList();
        }
        public void check()
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {

            }
        }
        public void RefreshUsersList()
        {
            UsersList = new List<UserDTO>();
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                foreach (User user in ctx.Users)
                {
                    UsersList.Add(Mapper.Map<UserDTO>(user));
                }
            }
        }

        public long Delete(UserDTO user)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (UsersList.Find(u => u.u_id == user.u_id) != null)
                {
                    try
                    {
                        ctx.Users.Remove(new User() { u_code = user.u_code, u_id = user.u_id, u_firstName = user.u_firstName.ToString(), u_lastName = user.u_lastName.ToString(), /*u_Email = user.Email,*/ u_password = user.u_password, u_permission = user.u_permission});
                        ctx.SaveChanges();

                        RefreshUsersList();
                    }
                    catch (Exception ex)
                    {
                        throw ex;

                    }
                }
            }
            return 1;

        }
        public List<UserDTO> GetAllUsers()
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                return Mapper.Map<List<UserDTO>>(ctx.Users);
            }
        }
        public long Insert(UserDTO user)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (UsersList.Find(u => u.u_id == user.u_id) == null)
                    try
                    {
                        ctx.Users.Add(Mapper.Map<User>(user));
                        ctx.SaveChanges();
                        RefreshUsersList();
                        return UsersList.First(u => u.u_id == user.u_id).u_code;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
            }
            return UsersList.First(u => u.u_id == user.u_id).u_code;
        }

        public UserDTO GetUserByCode( long code)
        {
            if (UsersList.Find(u => u.u_code == code) == null)
                return null;
            return UsersList.First(u => u.u_code == code);
        }
        public long GetUserById(string id)
        {
            if (UsersList.Find(u => u.u_id == id) == null)
                return 0;
            return UsersList.First(u => u.u_id == id).u_code;
        }

        public long Update(UserDTO user)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (UsersList.Find(u => u.u_id == user.u_id) == null)
                    try
                    {
                        ctx.Users.Add(Mapper.Map<User>(user));
                        ctx.SaveChanges();
                        RefreshUsersList();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
            }
            return 0;
        }
        public UserDTO GetUserByIdAndPassword(string id, string password)
        {
            if (UsersList.First(u => u.u_id == id).u_password == password)
                return UsersList.First(u => u.u_id == id);
            return null;
        }
    }
}