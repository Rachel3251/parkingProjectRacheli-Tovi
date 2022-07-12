using AutoMapper;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace BLL
{
    public  class UserPermissionBL
    {
        public List<UserPermissionDTO> UserPermissionsList;
        public UserPermissionBL()
        {           
            RefreshUserPermissionsList();
        }

        public void RefreshUserPermissionsList()
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                foreach (UserPermission userPermission in ctx.UserPermissions)
                {
                    UserPermissionsList.Add(Mapper.Map<UserPermissionDTO>(userPermission));
                }
            }

        }
        public List<UserPermissionDTO> GetAllUserPermissions()
        {
                return UserPermissionsList;
        }
        public UserPermissionDTO GetUserPermissionByCode(int code)
        {
            return UserPermissionsList.First(u => u.per_code == code);
        }
    }
}
