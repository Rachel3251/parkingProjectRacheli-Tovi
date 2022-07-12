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
    public class BlockBL
    {
        public static List<BlockDTO> BlocksList = new List<BlockDTO>();

        ParkingBL pBL = new ParkingBL();
        public BlockBL()
        {
            pBL = new ParkingBL();
            RefreshBlocksList();

        }

        public void RefreshBlocksList()
        {
            BlocksList = new List<BlockDTO>();
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                BlocksList = Mapper.Map<List<BlockDTO>>(ctx.Blocks);
                //plBL.RefreshLists();
            }
        }

        public long Insert(BlockDTO block, List<ParkingDTO> parkingsOfBlock)
        {
            long num=0;
            if (BlocksList.Find(u => u.b_parkingLotCode == block.b_parkingLotCode && u.b_codeName == block.b_codeName) == null)
                try
                {
                    //parkingsOfBlock.AddPasses(block);
                    block.b_legalParkings = parkingsOfBlock.Count(p => p.p_isLegal);
                    using (PARKINGEntities ctx = new PARKINGEntities())
                    {
                        Block block1 = new Block();
                        if (BlocksList.Count == 0)
                            block1.b_legalParkings = 0;
                        else
                            block1.b_code = BlocksList.Max(b => b.b_code) + 1;
                        block1.b_parkingLotCode = block.b_parkingLotCode;
                        block1.b_numberParkingsForI = block.b_numberParkingsForI;
                        block1.b_numberParkingsForJ = block.b_numberParkingsForJ;
                        block1.b_legalParkings = block.b_legalParkings;
                        block1.b_codeName = block.b_codeName;
                        block1.b_enteranceDirection = block.b_enteranceDirection;
                        ctx.Blocks.Add(block1);
                        RefreshBlocksList();
                        ctx.SaveChanges();
                        parkingsOfBlock.ForEach(p => p.p_blockNumber = block1.b_code);
                        //parkingsOfBlock.ForEach(p => p.p_blockNumber = BlocksList.FirstOrDefault(b => b.b_code == p.p_blockNumber).b_code);
                        pBL.InsertAllParkingsOfParkingLot(parkingsOfBlock);
                        ParkingLotBL.RefreshDictionary(block.b_parkingLotCode);
                        //ParkingLotBL.ParkingLotsList.FirstOrDefault(pl => pl.pl_code == block.b_parkingLotCode).pl_numberBlocks = BlocksList.Count(b => b.b_parkingLotCode == block.b_parkingLotCode);

                        //long num = BlocksList.FirstOrDefault(u => u.b_code == block1.b_code).b_code;
                        num = block1.b_code;
                        return num;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            //return BlocksList.FirstOrDefault(b => b.b_code == block.b_code).b_code;
            return num;

        }

        public long Update(BlockDTO block, List<ParkingDTO> parkingsOfBlock)
        {
            if (BlocksList.Find(u => u.b_code == block.b_code) != null)
                try
                {
                    parkingsOfBlock.NamingParkings();
                    //parkingsOfBlock.AddPasses(block);
                    block.b_legalParkings = parkingsOfBlock.Count(p => p.p_isLegal);
                    using (PARKINGEntities ctx = new PARKINGEntities())
                    {
                        //ctx.Blocks..Execute<Block>(new Block() { b_code = block.b_code, b_parkingLotCode = block.b_parkingLotCode, b_numberParkingsForI = block.b_numberParkingsForI, b_numberParkingsForJ = block.b_numberParkingsForJ, b_legalParkings = block.b_legalParkings, b_codeName = block.b_codeName, b_enteranceDirection = block.b_enteranceDirection });
                        RefreshBlocksList();
                        //pBL.Update(parkingsOfBlock);
                        //ParkingLotBL.RefreshDictionary(block.parkingLotCode);
                        return BlocksList.First(u => u.b_code == block.b_code).b_code;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }

            return BlocksList.First(u => u.b_code == block.b_code).b_code;
        }

        public int Delete(BlockDTO block)
        {
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                if (BlocksList.Find(u => u.b_code == block.b_code) != null)
                    try
                    {
                        ctx.Blocks.Remove(new Block() { b_code = block.b_code, b_parkingLotCode = block.b_parkingLotCode, b_numberParkingsForI = block.b_numberParkingsForI, b_numberParkingsForJ = block.b_numberParkingsForJ, b_legalParkings = block.b_legalParkings });
                        RefreshBlocksList();
                        //ParkingLotBL.ParkingLotsList.First(pl => pl.code == block.parkingLotCode).numberBlocks = BlocksList.Count(b => b.parkingLotCode == block.parkingLotCode);
                        //pBL.Delete(block.code);

                        return 0;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                //ParkingLotBL.RefreshDictionary(block.parkingLotCode);
                return 1;
            }
        }

        public List<BlockDTO> GetAllBlocks()
        {
            return BlocksList;
        }

        public List<BlockDTO> GetAllBlocksOfParkingLot(long parkingLotCode)
        {
            return BlocksList.Where(b => b.b_parkingLotCode == parkingLotCode).ToList();
        }

        public BlockDTO GetBlockByCode(long code)
        {
            return BlocksList.First(u => u.b_code == code);
        }
    }
}

