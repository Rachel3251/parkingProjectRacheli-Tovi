using AutoMapper;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using System.IO;
using System.Data.Entity.Validation;

namespace BLL
{
    public class ParkingLotBL
    {
        static string rulesForParkingLotOwners = @"..\..\..\RulesForPArkingLotsOwnersBeforeSigningParkingLot.txt";
        private UsersBL uBL;
        private BlockBL bBL;
        private ParkingBL pBL;
        public static List<ParkingLotDTO> ParkingLotsList;

        //public Dictionary<long, Dictionary<long, ParkingDTO[][]>> ParkingLotsDict { get => ParkingLotsDict; set => ParkingLotsDict = value; }
        public static Dictionary<long, Dictionary<long, ParkingDTO[][]>> ParkingLotsDict = new Dictionary<long, Dictionary<long, ParkingDTO[][]>>();

        public ParkingLotBL()
        {
            uBL = new UsersBL();
            bBL = new BlockBL();
            pBL = new ParkingBL();
            ParkingLotsDict = new Dictionary<long, Dictionary<long, ParkingDTO[][]>>();
            RefreshList();
            RefreshDictionary();
        }

        public void RefreshList()
        {
            ParkingLotsList = new List<ParkingLotDTO>();
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                ParkingLotsList = Mapper.Map<List<ParkingLotDTO>>(ctx.ParkingLots);
            }
        }

        public static void RefreshDictionary()
        {
            foreach (ParkingLotDTO pl in ParkingLotsList)
            {
                RefreshDictionary(pl.pl_code);
            }
        }

        public static void RefreshDictionary(long plCode)
        {
            ParkingDTO[][] mat=new ParkingDTO[][] {  };

            ParkingLotsDict[plCode] = new Dictionary<long, ParkingDTO[][]>();
            foreach (BlockDTO b in BlockBL.BlocksList.Where(b => b.b_parkingLotCode == plCode))
            {
                mat = new ParkingDTO[b.b_numberParkingsForI][];
                for (int i = 0; i < mat.Length; i++)
                {
                    mat[i] = new ParkingDTO[b.b_numberParkingsForJ];
                }
                ParkingLotsDict[plCode][(int)b.b_code] = mat;
            }
            //foreach (var i in ParkingBL.ParkingsList.Where(p => p.p_parkingLotCode == plCode))
            //{
            //    //ParkingLot[][] block;
            //    //var  parkingLot = ParkingLotsDict[plCode];
            //    //var bloisBlockck=  parkingLot.TryGetValue(i.p_blockNumber,out block)
            //    ParkingLotsDict[plCode][i.p_blockNumber][i.p_Location_i][i.p_Location_j] = i;
            //}
        }

        public long AddParkingLot(ParkingLotDTO parkingLot, List<BlockDTO> blocks, List<ParkingDTO> parkings)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                try
                {
                    ParkingLot parkingLot1 = new ParkingLot();
                    parkingLot1.pl_code = ParkingLotsList.Max(pl => pl.pl_code) + 1;

                    parkingLot1.pl_ownerCode = parkingLot.pl_ownerCode;
                    parkingLot1.pl_name = parkingLot.pl_name;
                    parkingLot1.pl_cityCode = parkingLot.pl_cityCode;
                    parkingLot1.pl_location = parkingLot.pl_location;
                    parkingLot1.pl_numberBlocks = parkingLot.pl_numberBlocks;
                    parkingLot1.pl_openHour = parkingLot.pl_openHour;
                    parkingLot1.pl_closeHour = parkingLot.pl_closeHour;
                    ctx.ParkingLots.Add(parkingLot1);
                    //ctx.ParkingLots.Add(new ParkingLot() { pl_code = ParkingLotsList.Max(pl => pl.pl_code) + 1, pl_ownerCode = parkingLot.pl_ownerCode, pl_name = parkingLot.pl_name, pl_cityCode = parkingLot.pl_cityCode, pl_location = parkingLot.pl_location, pl_numberBlocks = parkingLot.pl_numberBlocks, pl_openHour = parkingLot.pl_openHour, pl_closeHour = parkingLot.pl_closeHour });
                    ctx.SaveChanges();
                    blocks.ForEach(b => b.b_parkingLotCode = parkingLot1.pl_code);

                    parkings.ForEach(p => p.p_parkingLotCode = parkingLot1.pl_code);

                    bBL.Insert(blocks[0], parkings);
                    //foreach (Block block in blocks)
                    //{
                    // bBL.Insert(block, parkings.Where(p => p.p_blockNumber == block.b_codeName).ToList());
                    //}

                    ctx.SaveChanges();

                    RefreshList();
                    RefreshDictionary();

                    return ParkingLotsList.Max(pl => pl.pl_code);
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                //using (PARKINGEntities ctx = new PARKINGEntities())
                //{

                //    //if (ParkingLotsList.Find(pl => pl.pl_name == parkingLot.pl_name && pl.pl_location == parkingLot.pl_location && pl.pl_cityCode == parkingLot.pl_cityCode) == null)
                //    try
                //    {
                //        ctx.ParkingLots.Add(new ParkingLot() { pl_code = ParkingLotsList.Max(pl => pl.pl_code) + 1, pl_ownerCode = parkingLot.pl_ownerCode, pl_name = parkingLot.pl_name, pl_cityCode = parkingLot.pl_cityCode, pl_location = parkingLot.pl_location, pl_numberBlocks = blocks.Count, pl_openHour = parkingLot.pl_openHour, pl_closeHour = parkingLot.pl_closeHour });
                //        blocks.ForEach(b => b.b_parkingLotCode = parkingLot.pl_code);
                //        parkings.ForEach(p => p.p_parkingLotCode = parkingLot.pl_code);
                //        foreach (BlockDTO block in blocks)
                //        {
                //            bBL.Insert(block, parkings.Where(p => p.p_blockNumber == block.b_codeName).ToList());
                //        }
                //        ctx.SaveChanges();
                //        RefreshList();
                //        RefreshDictionary();

                //        return ParkingLotsList.Max(pl => pl.pl_code);
                //    }
                //    catch (Exception ex)
                //    {
                //        throw ex;
                //    }

            }
            return ParkingLotsList.FirstOrDefault(pl => pl.pl_code == parkingLot.pl_code).pl_code;
        }

        public long Update(ParkingLotDTO parkingLot)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                ParkingLot parking = ctx.ParkingLots.FirstOrDefault(p => p.pl_code == parkingLot.pl_code);
                parking.pl_cityCode = parkingLot.pl_cityCode;

            }
            //    if (ParkingLotsList.Find(pl => pl.pl_code == parkingLot.pl_code) != null)
            //        try
            //        {
            //            // dbCon.Execute<ParkingLot>(new ParkingLot() { pl_code = ParkingLotsList.Max(pl => pl.code) + 1, pl_name = parkingLot.name, pl_cityCode = parkingLot.cityCode, pl_location = parkingLot.location.ToString(), pl_numberParkingsForI = parkingLot.numberParkingsForI, pl_numberParkingsForJ = parkingLot.numberParkingsForJ }, DBConnection.ExecuteActions.Update);
            //            RefreshLists();
            //            return ParkingLotsList.First(pl => pl.pl_code == parkingLot.pl_code).pl_code;
            //        }
            //        catch (Exception ex)
            //        {
            //            return 0;
            //        }

            //    return ParkingLotsList.First(pl => pl.pl_code == parkingLot.pl_code).pl_code;
            return 0;
        }

        public int Delete(ParkingLotDTO parkingLot)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {

                if (ParkingLotsList.Find(pl => pl.pl_code == parkingLot.pl_code) != null)
                    try
                    {
                        ctx.ParkingLots.Remove(new ParkingLot() { pl_code = parkingLot.pl_code, pl_name = parkingLot.pl_name, pl_cityCode = parkingLot.pl_cityCode, pl_location = parkingLot.pl_location, pl_numberBlocks = parkingLot.pl_numberBlocks, pl_openHour = parkingLot.pl_openHour, pl_closeHour = parkingLot.pl_closeHour, pl_ownerCode = parkingLot.pl_ownerCode });
                        foreach (ParkingDTO i in ParkingBL.ParkingsList.Where(p => p.p_parkingLotCode == parkingLot.pl_code))
                        {
                            pBL.Delete(i);
                        }
                        RefreshList();
                        ParkingLotsDict.Remove(parkingLot.pl_code);
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        return 1;
                    }

                return 1;
            }
        }

        public List<ParkingLotDTO> GetAllParkingLots()
        {
            return ParkingLotsList;
        }

        public ParkingLotDTO GetParkingLotByCode(long code)
        {
            return ParkingLotsList.First(pl => pl.pl_code == code);
        }
        public List<ParkingLotDTO> GetAllParkingLotsInCity(long cityCode)
        {
            return ParkingLotsList.Where(pl => pl.pl_cityCode == cityCode).ToList();
        }

        public long GetParkingLotByOwnerId(string id)
        {
            return ParkingLotsList.First(pl => pl.pl_ownerCode == uBL.GetUserById(id)).pl_code;
        }

        public int[] NumberOfParkings(double height, double width)
        {
            return BuildParkingLotBL.NumberOfParkings(height, width);
        }

        public string GetRules()
        {
            return File.ReadAllText(rulesForParkingLotOwners);
        }

        public bool CheckParkingLotFitting(List<ParkingDTO> list)
        {

            List<ParkingDTO> listWithUpdatePasses = new List<ParkingDTO>();
            return false;
        }

        //public List<ParkingLotDTO> GetParkingLotbyCityBL(long code)
        //{
        //    List<ParkingLotDTO> Parkings = new List<ParkingLotDTO>();
        //    foreach (ParkingLotDTO parkimgLot in ParkingLotsList)
        //    {
        //       if(ParkingLotsList.Find(pl => pl.cityCode == code)!=null)
        //            Parking.Add()


        //    }


        //}

    }
    public class AddParkingLot
    {
        public ParkingLotDTO parkingLot { get; set; }
        public List<BlockDTO> blocks { get; set; }
        public List<ParkingDTO> parkings { get; set; }
    }
}

