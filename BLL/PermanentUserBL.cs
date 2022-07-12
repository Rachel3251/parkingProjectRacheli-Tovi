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
    public class PermanentUserBL
    {
        public List<PermanentUserDTO> PermanentUsersList = new List<PermanentUserDTO>();

        public PermanentUserBL()
        {
            RefreshPermanentUsersList();
        }

        public void RefreshPermanentUsersList()
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                foreach (PermanentUser permanentUser in ctx.PermanentUsers)
                {
                    PermanentUsersList.Add(Mapper.Map<PermanentUserDTO>(permanentUser));
                }
            }
        }

        public long Insert(PermanentUserDTO permanentUser)
        {
            PermanentUserDTO permanent = new PermanentUserDTO();
            permanent.cu_code = 1;
            permanent.cu_entranceHour = permanentUser.cu_entranceHour;
            permanent.cu_status = permanentUser.cu_status;
            permanent.cu_leavingHour = permanentUser.cu_leavingHour;
            permanent.cu_userCode = permanentUser.cu_userCode;
            permanent.cu_dayCode = permanentUser.cu_dayCode;
            permanent.cu_lastDate = permanentUser.cu_lastDate;
            permanent.cu_startDate = permanentUser.cu_startDate;
            permanent.cu_parkingLotCode = permanentUser.cu_parkingLotCode;
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                try
                {
                    //ctx.PermanentUsers.Add(new PermanentUser() { cu_code = PermanentUsersList.Max(c => c.cu_code) + 1, cu_userCode = permanentUser.cu_userCode, cu_parkingLotCode = permanentUser.cu_parkingLotCode, cu_dayCode = permanentUser.cu_dayCode, cu_entranceHour = permanentUser.cu_entranceHour, cu_leavingHour = permanentUser.cu_leavingHour, cu_startDate = permanentUser.cu_startDate, cu_lastDate = permanentUser.cu_lastDate, cu_status = permanentUser.cu_status });
                    ctx.PermanentUsers.Add(Mapper.Map<PermanentUser>(permanent));
                    ctx.SaveChanges();
                    RefreshPermanentUsersList();
                    //return PermanentUsersList.First(pu => pu.cu_code == permanentUser.cu_code).cu_code;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            return 100;

        }

        public long Update(PermanentUserDTO permanentUser)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (PermanentUsersList.Find(pu => pu.cu_code == permanentUser.cu_code) != null)
                    try
                    {
                        ctx.PermanentUsers.Add(new PermanentUser() { cu_code = permanentUser.cu_code, cu_userCode = permanentUser.cu_userCode, cu_parkingLotCode = permanentUser.cu_parkingLotCode, cu_dayCode = permanentUser.cu_dayCode, cu_entranceHour = permanentUser.cu_entranceHour, cu_leavingHour = permanentUser.cu_leavingHour, cu_startDate = permanentUser.cu_startDate, cu_lastDate = permanentUser.cu_lastDate, cu_status = permanentUser.cu_status });
                        ctx.SaveChanges();
                        RefreshPermanentUsersList();
                        return PermanentUsersList.First(pu => pu.cu_code == permanentUser.cu_code).cu_code;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }

                return PermanentUsersList.First(pu => pu.cu_code == permanentUser.cu_code).cu_code;
            }
        }

        public int Delete(PermanentUserDTO permanentUser)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (PermanentUsersList.Find(pu => pu.cu_code == permanentUser.cu_code) != null)
                    try
                    {
                        ctx.PermanentUsers.Remove(new PermanentUser() { cu_code = permanentUser.cu_code, cu_userCode = permanentUser.cu_userCode, cu_parkingLotCode = permanentUser.cu_parkingLotCode, cu_dayCode = permanentUser.cu_dayCode, cu_entranceHour = permanentUser.cu_entranceHour, cu_leavingHour = permanentUser.cu_leavingHour, cu_startDate = permanentUser.cu_startDate, cu_lastDate = permanentUser.cu_lastDate, cu_status = permanentUser.cu_status });
                        RefreshPermanentUsersList();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        return 1;
                    }

                return 1;
            }
        }

        public List<PermanentUserDTO> GetAllPermanentUsers()
        {
            return PermanentUsersList;
        }

        public PermanentUserDTO GetPermanentUsersByCode(long code)
        {
            return PermanentUsersList.First(pu => pu.cu_code == code);
        }
        public List<PermanentUserDTO> GetAllActualDaysInParkingLotByUserId(string id, long parkingLotCode)
        {
            return PermanentUsersList.Where(pu => UsersBL.UsersList.First(u => u.u_code == pu.cu_userCode).u_id == id
                    && pu.cu_parkingLotCode == parkingLotCode && pu.cu_startDate <= DateTime.Now.Date && pu.cu_lastDate >= DateTime.Now.Date).ToList();
        }

        public DateTime GetEnteranceHourOfPermanentUserToday(string id, long parkingLotCode)
        {
            return PermanentUsersList.First(pu => UsersBL.UsersList.First(u => u.u_code == pu.cu_userCode).u_id == id
                    && pu.cu_dayCode == DateTime.Now.Day && pu.cu_parkingLotCode == parkingLotCode).cu_entranceHour;
        }

        public DateTime GetLeavingHourOfPermanentUserToday(string id, long parkingLotCode)
        {
            return PermanentUsersList.First(pu => UsersBL.UsersList.First(u => u.u_code == pu.cu_userCode).u_id == id
                    && pu.cu_dayCode == DateTime.Now.Day && pu.cu_parkingLotCode == parkingLotCode).cu_leavingHour;
        }

        public long GetParkingLotCodeOfPermanentUserToday(string id)
        {
            return PermanentUsersList.First(pu => UsersBL.UsersList.First(u => u.u_code == pu.cu_code).u_id == id
                    && pu.cu_dayCode == DateTime.Now.Day).cu_parkingLotCode;
        }
    }
}
