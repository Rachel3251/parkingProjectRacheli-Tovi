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
    [RoutePrefix("api/block")]
    public class BlockController : ApiController
    {
        BLL.BlockBL blockBL = new BLL.BlockBL();

        [AcceptVerbs("GET", "POST")]

        //הוספת בלוק עם כל החניות שלו
        [Route("insert")]
        [HttpPost]
        public long Insert(BlockDTO block, List<ParkingDTO> parkingsOfBlock)
        {
            return blockBL.Insert(block, parkingsOfBlock);
        }

        //עדכון בלוק עם כל החניות שלו
        [Route("update")]
        [HttpPost]
        public long Update(BlockDTO block, List<ParkingDTO> parkingsOfBlock)
        {
            return blockBL.Update(block, parkingsOfBlock);
        }

        //מחיקת בלוק ואת כל החניות שלו
        [Route("delete")]
        [HttpPost]
        public long Delete(BlockDTO block)
        {
            return blockBL.Delete(block);
        }

        //קבלת כל הבלוקים של חניון
        [Route("getallblocksofparkinglot")]
        [HttpGet]
        public List<BlockDTO> GetAllBlocksOfParkingLot(long code)
        {
            return blockBL.GetAllBlocksOfParkingLot(code);
        }

        [Route("getallblocks")]
        [HttpGet]
        public List<BlockDTO> GetAllBlocks()
        {
            return blockBL.GetAllBlocks();
        }

        [Route("getblockbycode")]
        [HttpGet]
        public BlockDTO GetBlockByCode(long code)
        {
            return blockBL.GetBlockByCode(code);
        }

        
    }
}