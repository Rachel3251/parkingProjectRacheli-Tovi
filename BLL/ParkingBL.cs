using AutoMapper;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using System.Data.Entity.Validation;

namespace BLL
{
  
    public class ParkingBL
    {
        public static List<ParkingDTO> ParkingsList = new List<ParkingDTO>();
        //ParkingLotBL plBL;

        public ParkingBL()
        {
            //plBL = new ParkingLotBL();
            RefreshParkingsList();
            //plBL.RefreshDictionary();
        }

        public void RefreshParkingsList()
        {
            ParkingsList = new List<ParkingDTO>();
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                foreach (Parking parking in ctx.Parkings)
                {
                    ParkingsList.Add(Mapper.Map<ParkingDTO>(parking));
                }
                //ParkingsList = Mapper.Map<List<ParkingDTO>>(ctx.Parkings);
                //plBL.RefreshLists();
                //plBL.RefreshDictionary();
            }
        }

        public long InsertAllParkingsOfParkingLot(List<ParkingDTO> parkingsOfBlock)
        {

            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                long code = 0;
                foreach (ParkingDTO parking in parkingsOfBlock)
                {
                    switch (parking.p_status)
                    {
                        case "red":
                            parking.p_status ="ILLEGAL";
                            break;
                        case "green":
                            parking.p_status = "ENTERANCE";
                            break;
                        case "yellow":
                            parking.p_status = "EXIT";
                            break;
                        default:
                            parking.p_status ="PARKING";
                            break;
                    }
                    if (ParkingsList.Find(p => p.p_code == parking.p_code) == null)
                        try
                        {
                            Parking parking1 = new Parking();
                            if (ParkingsList.Count == 0)
                                parking1.p_code = 1;
                            else
                                parking1.p_code = ParkingsList.Max(c => c.p_code) + 1;
                            parking1.p_parkingLotCode = parking.p_parkingLotCode;
                            parking1.p_Location_i = parking.p_Location_i;
                            parking1.p_Location_j = parking.p_Location_j;
                            parking1.p_isLegal = parking.p_isLegal;
                            parking1.p_name = parking.p_name;
                            parking1.p_blockNumber = parking.p_blockNumber;
                            parking1.p_status = parking.p_status;
                            ctx.Parkings.Add(parking1);
                            ctx.SaveChanges();
                            RefreshParkingsList();
                            code = ParkingsList.FirstOrDefault(p => p.p_code == parking1.p_code).p_code;
                        }
                        catch (Exception ex)
                        {
                            return 0;
                        }

                }
                return code;
            }
        }

        //public long Insert(ParkingDTO parking)
        //{
        //    using (PARKINGEntities ctx = new PARKINGEntities())
        //    {
        //        if (ParkingsList.Find(p => p.p_code == parking.p_code) == null)
        //            try
        //            {
        //                ctx.Parkings.Add(new Parking() { p_code = ParkingsList.Max(c => c.p_code) + 1, p_parkingLotCode = parking.parkingLotCode, p_Location_i = parking.Location_i, p_Location_j = parking.Location_j, p_isLegal = parking.isLegal });
        //                ctx.SaveChanges();
        //                RefreshParkingsList();
        //                //plBL.RefreshDictionary(parking.p_parkingLotCode);
        //                return ParkingsList.First(p => p.p_code == parking.p_code).code;
        //            }
        //            catch (Exception ex)
        //            {
        //                return 0;
        //            }

        //        return ParkingsList.First(p => p.p_code == parking.p_code).p_code;
        //    }
        //}

        public long Update(ParkingDTO parking)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (ParkingsList.Find(p => p.p_code == parking.p_code) != null)
                    try
                    {
                        ctx.Parkings.Add(new Parking() { p_code = parking.p_code, p_parkingLotCode = parking.p_parkingLotCode, p_Location_i = parking.p_Location_i, p_Location_j = parking.p_Location_j, p_isLegal = parking.p_isLegal });
                        ctx.SaveChanges();
                        RefreshParkingsList();
                        //plBL.RefreshDictionary(parking.p_parkingLotCode);
                        return ParkingsList.First(p => p.p_code == parking.p_code).p_code;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }

                return ParkingsList.First(p => p.p_code == parking.p_code).p_code;
            }
        }

        public int Delete(ParkingDTO parking)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (ParkingsList.Find(p => p.p_code == parking.p_code) != null)
                    try
                    {
                        ctx.Parkings.Remove(new Parking() { p_code = parking.p_code, p_parkingLotCode = parking.p_parkingLotCode, p_Location_i = parking.p_Location_i, p_Location_j = parking.p_Location_j, p_isLegal = parking.p_isLegal });
                        ctx.SaveChanges();
                        RefreshParkingsList();
                        //plBL.RefreshDictionary(parking.p_parkingLotCode);
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        return 1;
                    }

                return 1;
            }
        }
        //כיון שבכל פעם מבצעים בדיקת תאימות לפני שמוחקים חניה, אין צורך למחוק חניון כשיגמרו החניות

        public List<ParkingDTO> GetAllParkings()
        {
            return ParkingsList;
        }

        public ParkingDTO GetParkingByCode(long code)
        {
            return ParkingsList.First(p => p.p_code == code);
        }

        public List<ParkingDTO> GetAllParkingsOfBlock(long code)
        {
            return ParkingsList.Where(p => p.p_blockNumber == code).ToList();
        }
    }
}
