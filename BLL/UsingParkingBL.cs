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
   public class UsingParkingBL
    {
        public List<UsingParkingDTO> UsingParkingList;
        public UsingParkingBL()
        {
            RefreshUsingParkingList();
        }
        public void RefreshUsingParkingList()
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                foreach (UsingParking usingParking in ctx.UsingParkings)
                {
                    UsingParkingList.Add(Mapper.Map<UsingParkingDTO>(usingParking));
                }
            }
        }
        public long Insert(UsingParkingDTO usingParking)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (UsingParkingList.Find(u => u.up_code == usingParking.up_code) == null)
                    try
                    {
                        ctx.UsingParkings.Add(new UsingParking() { up_code = UsingParkingList.Max(c => c.up_code) + 1, up_parkingCode = usingParking.up_parkingCode, up_isUsing = usingParking.up_isUsing, up_date = usingParking.up_date, up_entranceHour = usingParking.up_entranceHour, up_leavingHour = usingParking.up_leavingHour});
                        ctx.SaveChanges();
                        RefreshUsingParkingList();
                        return UsingParkingList.First(u => u.up_code == usingParking.up_code).up_code;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }

                return UsingParkingList.First(u => u.up_code == usingParking.up_code).up_code;
            }
        }

        public long Update(UsingParkingDTO usingParking)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (UsingParkingList.Find(u => u.up_code == usingParking.up_code) != null)
                    try
                    {
                        ctx.UsingParkings.Add(new UsingParking() { up_code = usingParking.up_code, up_parkingCode = usingParking.up_parkingCode, up_isUsing = usingParking.up_isUsing, up_date = usingParking.up_date, up_entranceHour = usingParking.up_entranceHour, up_leavingHour = usingParking.up_leavingHour});
                        ctx.SaveChanges();
                        RefreshUsingParkingList();
                        return UsingParkingList.First(u => u.up_code == usingParking.up_code).up_code;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }

                return UsingParkingList.First(u => u.up_code == usingParking.up_code).up_code;
            }
        }
        public int Delete(UsingParkingDTO usingParking)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (UsingParkingList.Find(u => u.up_code == usingParking.up_code) != null)
                    try
                    {
                        ctx.UsingParkings.Remove(new UsingParking() { up_code = usingParking.up_code, up_parkingCode = usingParking.up_parkingCode, up_isUsing = usingParking.up_isUsing, up_date = usingParking.up_date, up_entranceHour = usingParking.up_entranceHour, up_leavingHour = usingParking.up_leavingHour});
                        ctx.SaveChanges();
                        RefreshUsingParkingList();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        return 1;
                    }

                return 1;
            }

        }
        public List<UsingParkingDTO> GetAllUsingParking()
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                return Mapper.Map<List<UsingParkingDTO>>(ctx.UsingParkings);
            }
        }

        public UsingParkingDTO GetUsingParkingByCode(long code)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                return UsingParkingList.First(u => u.up_code == code);
            }
        }

    }
}
