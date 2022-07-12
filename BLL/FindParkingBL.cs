using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Models;


namespace BLL
{
    public static class FindParkingBL
    {
        public static ParkingBL pBL = new ParkingBL();
        public static BlockBL bBL = new BlockBL();
        //Details about this current filling
        public static Dictionary<long, Dictionary<long, Details>> details;
        //Real Using Parkings matrix
        public static Dictionary<long, Dictionary<long, List<List<List<UsingParkingDTO>>>>> UsedParkingsDict;
        public static bool x { get; set; } = true;
        //אם נגמרו החונים המשוערים והגיע עוד רכב, צריך לקרוא מכאן לבד לפונקציה addEmptyAreas()
        //Finding optimal parking
        //

        public static bool HasRoute(this UsingParkingDTO usingParking, UsingParkingDTO request)
        {
            ParkingDTO parking = pBL.GetParkingByCode(usingParking.up_parkingCode);
            long i = 0;
            if (parking.p_Location_j == details[parking.p_parkingLotCode][parking.p_blockNumber].currentLeftColumn)
            {
                if (parking.p_Location_j == 0) return false;
                for (i = parking.p_Location_i; i >= details[parking.p_parkingLotCode][parking.p_blockNumber].leftFront
                    && !UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)(parking.p_Location_j - 1)][(int)i].up_isUsing; i--) ;
                return request.up_leavingHour.TimeOfDay > UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                    [(int)(parking.p_Location_j - 1)][(int)i].up_leavingHour.TimeOfDay;
            }
            if (parking.p_Location_j == bBL.GetBlockByCode(parking.p_blockNumber).b_numberParkingsForJ - 1) return false;
            for (i = parking.p_Location_i; i >= details[parking.p_parkingLotCode][parking.p_blockNumber].rightFront
                && !UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)(parking.p_Location_j + 1)][(int)i].up_isUsing; i--) ;
            return request.up_leavingHour.TimeOfDay > UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                [(int)(parking.p_Location_j + 1)][(int)i].up_leavingHour.TimeOfDay;
        }

        //This function is happens only on the part of the block whose belong to this current filling.
        public static List<UsingParkingDTO> AllFitParkingsInColumn(long parkingLotCode, long blockCode, long column, UsingParkingDTO request)
        {
            if (column == details[parkingLotCode][blockCode].currentLeftColumn)
                return ResetHours.PredictedUsedParkingsDict[parkingLotCode][blockCode].Last()[(int)column]
                    .GetRange((int)details[parkingLotCode][blockCode].leftFront, (int)details[parkingLotCode][blockCode].leftBack
                    - (int)details[parkingLotCode][blockCode].leftFront).Where(up => up.IsInRange(request)
                    && !UsedParkingsDict[parkingLotCode][blockCode].Last()[(int)pBL.GetParkingByCode(up.up_parkingCode)
                    .p_Location_j][(int)pBL.GetParkingByCode(up.up_parkingCode).p_Location_i].up_isUsing
                    && pBL.GetParkingByCode(up.up_parkingCode).p_isLegal).ToList();
            return ResetHours.PredictedUsedParkingsDict[parkingLotCode][blockCode].Last()[(int)column]
                    .GetRange((int)details[parkingLotCode][blockCode].rightFront, (int)details[parkingLotCode][blockCode].rightBack
                    - (int)details[parkingLotCode][blockCode].rightFront).Where(up => up.IsInRange(request)
                    && !UsedParkingsDict[parkingLotCode][blockCode].Last()[(int)pBL.GetParkingByCode(up.up_parkingCode)
                    .p_Location_j][(int)pBL.GetParkingByCode(up.up_parkingCode).p_Location_i].up_isUsing
                    && pBL.GetParkingByCode(up.up_parkingCode).p_isLegal).ToList();
        }
        public static bool IsInRange(this UsingParkingDTO usingParking, UsingParkingDTO request)
        {
            //:כל האופציות
            //הקדמי ריק והאחורי לא חוקי P (עושים לולאה עד שמגיעים למיקום חוקי ועליו בודקים את כל הסוגים, לפני הכל)
            //הקדמי ריק והאחורי ריק P P
            //הקדמי ריק והאחורי מלא P M
            //הקדמי ריק והאחורי מחוץ לגבול P D
            //הקדמי ריק והאחורי מוקדם יותר P M

            //הקדמי מלא והאחורי ריק
            //הקדמי מלא והאחורי מלא
            //הקדמי מלא והאחורי מחוץ לגבול
            //הקדמי מלא והאחורי מוקדם יותר

            //הקדמי מחוץ לגבול והאחורי ריק
            //הקדמי מחוץ לגבול והאחורי מלא
            //הקדמי מחוץ לגבול והאחורי מוקדם יותר

            //הקדמי מאוחר יותר והאחורי ריק
            //הקדמי מאוחר יותר והאחורי מלא
            //הקדמי מאוחר יותר והאחורי מחוץ לגבול
            //הקדמי מאוחר יותר והאחורי מוקדם יותר

            ParkingDTO parking = pBL.GetParkingByCode(usingParking.up_parkingCode);

            DateTime frontHour;
            DateTime backHour;
            long frontRow = parking.p_Location_j < (bBL.GetBlockByCode(parking.p_blockNumber).b_numberParkingsForJ / 2) ?
                details[parking.p_parkingLotCode][parking.p_blockNumber].leftFront : details[parking.p_parkingLotCode][parking.p_blockNumber].rightFront;
            long backRow = parking.p_Location_j < (bBL.GetBlockByCode(parking.p_blockNumber).b_numberParkingsForJ / 2) ?
                details[parking.p_parkingLotCode][parking.p_blockNumber].leftBack : details[parking.p_parkingLotCode][parking.p_blockNumber].rightBack;
            long index;
            if (UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)parking.p_Location_j][(int)parking.p_Location_i].up_isUsing)
                return false;
            //Front parking
            if (parking.p_Location_i == frontRow)
            {
                frontHour = request.up_leavingHour;
            }
            else
            {

                switch (UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)parking.p_Location_j][(int)parking.p_Location_i - 1].up_isUsing)
                {
                    case false:
                        frontHour = ResetHours.PredictedUsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                            [(int)parking.p_Location_j][(int)parking.p_Location_i - 1].up_leavingHour;
                        break;
                    default:
                        index = Math.Max(pBL.GetParkingByCode(UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                            [(int)parking.p_Location_j].GetRange((int)frontRow, (int)(parking.p_Location_i - frontRow))
                            .LastOrDefault(up => up.up_isUsing && /*The last that in its place without routes*/up.up_leavingHour.TimeOfDay
                            <= UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber]
                            .Last()[(int)parking.p_Location_j][(int)parking.p_Location_i].up_leavingHour.TimeOfDay).up_parkingCode).p_Location_i,
                            pBL.GetParkingByCode(UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                            [(int)parking.p_Location_j].GetRange((int)frontRow, (int)(parking.p_Location_i - frontRow))
                            .LastOrDefault(up => !up.up_isUsing).up_parkingCode).p_Location_i);
                        switch (UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)parking.p_Location_j][(int)index].up_isUsing)
                        {
                            case true:
                                frontHour = UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                                    [(int)parking.p_Location_j][(int)index].up_leavingHour;
                                break;
                            default:
                                frontHour = ResetHours.PredictedUsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                                    [(int)parking.p_Location_j][(int)index].up_leavingHour;
                                break;
                        }
                        break;
                }

            }

            //Back parking
            if (parking.p_Location_i == backRow)
            {
                backHour = request.up_leavingHour;
            }
            else
            {
                switch (UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)parking.p_Location_j][(int)parking.p_Location_i + 1].up_isUsing)
                {
                    case false:
                        backHour = ResetHours.PredictedUsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                            [(int)parking.p_Location_j][(int)(parking.p_Location_i + 1)].up_leavingHour;
                        break;
                    default:
                        index = Math.Max(pBL.GetParkingByCode(UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                            [(int)parking.p_Location_j].GetRange((int)(parking.p_Location_i + 1), (int)backRow - (int)parking.p_Location_i)
                            .FirstOrDefault(up => up.up_isUsing && /*The first that in its place without routes*/up.up_leavingHour.TimeOfDay
                            >= UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber]
                            .Last()[(int)parking.p_Location_j][(int)parking.p_Location_i].up_leavingHour.TimeOfDay).up_parkingCode).p_Location_i,
                            pBL.GetParkingByCode(UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                            [(int)parking.p_Location_j].GetRange((int)(parking.p_Location_i + 1), (int)backRow - (int)parking.p_Location_i)
                            .FirstOrDefault(up => !up.up_isUsing).up_parkingCode).p_Location_i);
                        switch (UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()[(int)parking.p_Location_j][(int)index].up_isUsing)
                        {
                            case true:
                                backHour = UsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                                    [(int)parking.p_Location_j][(int)index].up_leavingHour;
                                break;
                            default:
                                backHour = ResetHours.PredictedUsedParkingsDict[parking.p_parkingLotCode][parking.p_blockNumber].Last()
                                    [(int)parking.p_Location_j][(int)index].up_leavingHour;
                                break;
                        }
                        break;
                }
            }
            return request.up_leavingHour.TimeOfDay >= frontHour.TimeOfDay && request.up_leavingHour.TimeOfDay <= backHour.TimeOfDay;
        }
        public static void UpdateStatistics(UsingParkingDTO request) { }
        ////leaving hours
        //public class Range
        //{
        //    public DateTime earliestHour;
        //    public DateTime latestHour;
        //}
        public static ParkingDTO FindParking(this ParkingLotDTO parkingLot, UsingParkingDTO request, bool status/*true: in the first time, false: additional filling*/)
        {
            if (!x)
            {
                foreach (KeyValuePair<long, Details> block in details[parkingLot.pl_code])
                {
                    if (UsedParkingsDict[parkingLot.pl_code][block.Key].Last()
                        [(int)details[parkingLot.pl_code][block.Key].currentLeftColumn].All(up => up.up_isUsing))
                    {
                        details[parkingLot.pl_code][block.Key].currentLeftColumn++;
                    }
                    if (UsedParkingsDict[parkingLot.pl_code][block.Key].Last()
                        [(int)details[parkingLot.pl_code][block.Key].currentRightColumn].All(up => up.up_isUsing))
                    {
                        details[parkingLot.pl_code][block.Key].currentRightColumn++;
                    }
                }

                #region //
                //הראשוניים
                #endregion
                List<UsingParkingDTO> AllFitParking1;


                #region Optimization number 1: Exchanges

                #region //
                //המיקומים המתאימים בעמודה הנוכחית אליו יכנס הראשון מהעמודה הסמוכה
                #endregion
                List<UsingParkingDTO> AllFitParking2;
                #region //
                //חניה שיכולה להחליף
                #endregion
                UsingParkingDTO exchangedParking;
                #region //
                //הראשון בעמודה שלפני הנוכחית
                #endregion
                UsingParkingDTO FirstParking;

                foreach (BlockDTO block in bBL.GetAllBlocks().OrderByDescending(b => details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces))
                {
                    //left side
                    AllFitParking1 =
                        AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, details[parkingLot.pl_code][block.b_code].currentLeftColumn, request);
                    if (AllFitParking1 != null)
                    {
                        long i, j = 0;
                        for (i = details[parkingLot.pl_code][block.b_code].leftFront;
                            i < pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i - 1; i++)
                        {
                            for (j = (int)(block.b_numberParkingsForJ / 2); j >= details[parkingLot.pl_code][block.b_code].currentLeftColumn + 2
                                && !UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)(j - 1)][(int)i].up_isUsing; j--) ;
                            //מחליפים עם שורה לפני השורה שבה הסמוך קטן או שווה לחניה להחלפה
                            if (request.up_leavingHour.TimeOfDay > UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                [(int)(j - 1)][(int)i].up_leavingHour.TimeOfDay)
                            {
                                if (i > details[parkingLot.pl_code][block.b_code].leftFront)
                                    i--;
                                break;
                            }//V
                        }
                        //If the found parking is later, and all the front parkings are earlier.
                        if (i < pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i &&
                                ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)j][(int)i].up_leavingHour.TimeOfDay
                                > AllFitParking1[0].up_leavingHour.TimeOfDay && UsedParkingsDict[parkingLot.pl_code][block.b_code]
                                .Last()[(int)j].GetRange((int)details[parkingLot.pl_code][block.b_code].leftFront,
                                (int)(i - details[parkingLot.pl_code][block.b_code].leftFront)).All(h => (h.up_isUsing &&
                                h.up_leavingHour.TimeOfDay <= request.up_leavingHour.TimeOfDay) || !pBL.GetParkingByCode(h.up_parkingCode).p_isLegal))
                        {
                            exchangedParking =
                                UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)j][(int)i];
                            //בודק האם יש בעמודה הנוכחית אפשרות להכניס את הראשון שמגיע אל העמודה הבאה
                            FirstParking =
                                UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code]
                                .currentLeftColumn + 1].GetRange((int)details[parkingLot.pl_code][block.b_code].leftFront,
                                (int)(details[parkingLot.pl_code][block.b_code].leftBack - details[parkingLot.pl_code][block.b_code].leftFront))
                                .Where(w => !w.up_isUsing).FirstOrDefault(f => ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                [(int)pBL.GetParkingByCode(f.up_parkingCode).p_Location_i][(int)pBL.GetParkingByCode(f.up_parkingCode).p_Location_i]
                                .up_entranceHour ==
                                UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code]
                                .currentLeftColumn + 1].GetRange((int)details[parkingLot.pl_code][block.b_code].leftFront,
                                (int)(details[parkingLot.pl_code][block.b_code].leftBack - details[parkingLot.pl_code][block.b_code].leftFront))
                                .Where(d => !d.up_isUsing).Max(m => ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                [(int)pBL.GetParkingByCode(m.up_parkingCode).p_Location_i][(int)pBL.GetParkingByCode(m.up_parkingCode).p_Location_i]
                                .up_entranceHour));
                            AllFitParking2 =
                                AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, details[parkingLot.pl_code]
                                [block.b_code].currentLeftColumn, FirstParking);
                            if (AllFitParking2 != null)
                            {
                                //מציאת הטווח של שתי החניות בעמודה - מי לפני מי ובדיקה אם כולם ריקים בטווח הזה
                                if (pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i
                                    < pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i
                                    && UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code]
                                    [block.b_code].currentLeftColumn].GetRange((int)pBL.GetParkingByCode(AllFitParking1
                                    [AllFitParking1.Count - 1].up_parkingCode).p_Location_i,
                                    (int)((int)pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i) -
                                    (int)pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i)
                                    .All(a => !a.up_isUsing) ||
                                    pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i
                                    > pBL.GetParkingByCode(AllFitParking2[AllFitParking2.Count - 1].up_parkingCode).p_Location_i
                                    && UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code]
                                    [block.b_code].currentLeftColumn].GetRange((int)pBL.GetParkingByCode(AllFitParking2
                                    [AllFitParking2.Count - 1].up_parkingCode).p_Location_i,
                                    (int)pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i -
                                    (int)pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i)
                                    .All(a => !a.up_isUsing))
                                {
                                    //עובר על כל העמודות להעברות
                                    //Do the exchange. In the future we can add priorities between the halves blocks.
                                    for (long g = details[parkingLot.pl_code][block.b_code].currentLeftColumn;
                                        g < pBL.GetParkingByCode(exchangedParking.up_parkingCode).p_Location_j - 1; g++)
                                    {
                                        //הריקן קטן יותר
                                        if (pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i
                                            < pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i)
                                        {
                                            for (long h = pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i;
                                            h < pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i; h++)
                                            {
                                                ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                                    [(int)g][(int)h] = ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                                    [(int)g][(int)h + 1];
                                            }
                                            ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)g]
                                                [(int)pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i] = FirstParking;
                                        }
                                        //המיקום המתאים לחניה שעוברת לעמודה זו קטן יותר
                                        if (pBL.GetParkingByCode(AllFitParking2[AllFitParking2.Count - 1].up_parkingCode).p_Location_i
                                            < pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i)
                                        {
                                            for (long h = pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i;
                                            h > pBL.GetParkingByCode(AllFitParking2[AllFitParking2.Count - 1].up_parkingCode).p_Location_i; h--)
                                            {
                                                ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)g][(int)h]
                                                    = ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                                    [(int)g][(int)h - 1];
                                            }
                                            ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)g]
                                                [(int)pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i] = FirstParking;
                                        }
                                    }
                                    request.up_isUsing = true;
                                    UpdateStatistics(request);
                                    UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)pBL.GetParkingByCode(exchangedParking.up_parkingCode).p_Location_j]
                                        [(int)pBL.GetParkingByCode(exchangedParking.up_parkingCode).p_Location_i] = request;
                                    return pBL.GetParkingByCode(exchangedParking.up_parkingCode);
                                }
                            }
                        }
                    }

                    //right side
                    AllFitParking1 =
                        AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, details[parkingLot.pl_code][block.b_code].currentRightColumn, request);
                    if (AllFitParking1 != null)
                    {
                        long i, j = 0;
                        for (i = details[parkingLot.pl_code][block.b_code].rightFront;
                           i < pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i - 1; i++)
                        {
                            for (j = (int)(block.b_numberParkingsForJ / 2); j <= details[parkingLot.pl_code][block.b_code].currentRightColumn + 2
                                && !UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)(j + 1)][(int)i].up_isUsing; j++) ;
                            //מחליפים עם שורה לפני השורה שבה הסמוך קטן או שווה לחניה להחלפה
                            if (request.up_leavingHour.TimeOfDay > UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                [(int)(j + 1)][(int)i].up_leavingHour.TimeOfDay)
                            {
                                if (i > details[parkingLot.pl_code][block.b_code].rightFront)
                                    i--;
                                break;
                            }
                        }
                        exchangedParking =
                            UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)j][(int)i];
                        //If the found parking is later, and all the front parkings are earlier.
                        if (ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)j][(int)i].up_leavingHour.TimeOfDay
                                > AllFitParking1[0].up_leavingHour.TimeOfDay && UsedParkingsDict[parkingLot.pl_code][block.b_code]
                                .Last()[(int)j].GetRange((int)details[parkingLot.pl_code][block.b_code].rightFront,
                                (int)(i - details[parkingLot.pl_code][block.b_code].rightFront)).All(h => (h.up_isUsing &&
                                h.up_leavingHour.TimeOfDay <= request.up_leavingHour.TimeOfDay) || !pBL.GetParkingByCode(h.up_parkingCode).p_isLegal))
                        {
                            //בודק האם יש בעמודה הנוכחית אפשרות להכניס את הראשון שמגיע אל העמודה הבאה
                            FirstParking =
                                UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code]
                                .currentRightColumn + 1].GetRange((int)details[parkingLot.pl_code][block.b_code].rightFront,
                                (int)(details[parkingLot.pl_code][block.b_code].rightBack - details[parkingLot.pl_code][block.b_code].rightFront))
                                .Where(w => !w.up_isUsing).FirstOrDefault(f => ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                [(int)pBL.GetParkingByCode(f.up_parkingCode).p_Location_i][(int)pBL.GetParkingByCode(f.up_parkingCode).p_Location_i]
                                .up_entranceHour ==
                                UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code]
                                .currentRightColumn + 1].GetRange((int)details[parkingLot.pl_code][block.b_code].rightFront,
                                (int)(details[parkingLot.pl_code][block.b_code].rightBack - details[parkingLot.pl_code][block.b_code].rightFront))
                                .Where(d => !d.up_isUsing).Max(m => ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                [(int)pBL.GetParkingByCode(m.up_parkingCode).p_Location_i][(int)pBL.GetParkingByCode(m.up_parkingCode).p_Location_i]
                                .up_entranceHour));
                            AllFitParking2 =
                                AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, details[parkingLot.pl_code]
                                [block.b_code].currentRightColumn, FirstParking);
                            if (AllFitParking2 != null)
                            {
                                //מציאת הטווח של שתי החניות בעמודה - מי לפני מי ובדיקה אם כולם ריקים בטווח הזה
                                if (pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i
                                    < pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i
                                    && UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code]
                                    [block.b_code].currentRightColumn].GetRange((int)pBL.GetParkingByCode(AllFitParking1
                                    [AllFitParking1.Count - 1].up_parkingCode).p_Location_i,
                                    (int)((int)pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i) -
                                    (int)pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i)
                                    .All(a => !a.up_isUsing) ||
                                    pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i
                                    > pBL.GetParkingByCode(AllFitParking2[AllFitParking2.Count - 1].up_parkingCode).p_Location_i
                                    && UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code]
                                    [block.b_code].currentRightColumn].GetRange((int)pBL.GetParkingByCode(AllFitParking2
                                    [AllFitParking2.Count - 1].up_parkingCode).p_Location_i,
                                    (int)((int)pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i) -
                                    (int)pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i)
                                    .All(a => !a.up_isUsing))
                                {
                                    //עובר על כל העמודות להעברות
                                    //Do the exchange. In the future we can add priorities between the halves blocks.
                                    for (long g = details[parkingLot.pl_code][block.b_code].currentRightColumn;
                                        g > pBL.GetParkingByCode(exchangedParking.up_parkingCode).p_Location_j - 1; g--)
                                    {
                                        //(הריקן קטן יותר (קרוב יותר ליציאה
                                        if (pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i
                                            < pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i)
                                        {
                                            for (long h = pBL.GetParkingByCode(AllFitParking1[AllFitParking1.Count - 1].up_parkingCode).p_Location_i;
                                            h < pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i; h++)
                                            {
                                                ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                                    [(int)g][(int)h] = ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                                    [(int)g][(int)h + 1];
                                            }
                                            ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)g]
                                                [(int)pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i] = FirstParking;
                                        }
                                        //המיקום המתאים לחניה שעוברת לעמודה זו קטן יותר
                                        if (pBL.GetParkingByCode(AllFitParking2[AllFitParking2.Count - 1].up_parkingCode).p_Location_i
                                            < pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i)
                                        {
                                            for (long h = pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode).p_Location_i;
                                            h > pBL.GetParkingByCode(AllFitParking2[AllFitParking2.Count - 1].up_parkingCode).p_Location_i; h--)
                                            {
                                                ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)g][(int)h]
                                                    = ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                                                    [(int)g][(int)h - 1];
                                            }
                                            ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)g]
                                                [(int)pBL.GetParkingByCode(AllFitParking2[0].up_parkingCode).p_Location_i] = FirstParking;
                                        }
                                    }
                                    request.up_isUsing = true;
                                    UpdateStatistics(request);
                                    UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)pBL.GetParkingByCode(exchangedParking.up_parkingCode).p_Location_j]
                                        [(int)pBL.GetParkingByCode(exchangedParking.up_parkingCode).p_Location_i] = request;
                                    return pBL.GetParkingByCode(exchangedParking.up_parkingCode);
                                }
                            }
                        }
                    }
                }
                #endregion


                #region Optimization number 2: Regulars
                UsingParkingDTO temp1 = null, temp2 = null;
                foreach (BlockDTO block in bBL.GetAllBlocks().OrderByDescending(b => details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces))
                {
                    AllFitParking1 = AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, details[parkingLot.pl_code][block.b_code]
                        .currentLeftColumn, request);
                    if (AllFitParking1 != null)
                    {
                        temp1 = AllFitParking1[AllFitParking1.Count / 2];
                    }
                    AllFitParking1 = AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, details[parkingLot.pl_code][block.b_code]
                        .currentRightColumn, request);
                    if (AllFitParking1 != null)
                    {
                        temp2 = AllFitParking1[AllFitParking1.Count / 2];
                    }
                    try
                    {
                        //בדיקה של הפרש השעה המשוערת מהבקשה עבור כל צד.
                        /*if(temp1.up_leavingHour.TimeOfDay > request.up_leavingHour.TimeOfDay &&)*///הסבר: תמיד הבקשה תהיה גדולה או שווה למקום שהתאים לה... אוקי
                        if (request.up_leavingHour.TimeOfDay - temp1.up_leavingHour.TimeOfDay
                            < request.up_leavingHour.TimeOfDay - temp2.up_leavingHour.TimeOfDay)
                        {
                            request.up_isUsing = true;
                            UpdateStatistics(request);
                            UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)pBL.GetParkingByCode
                                (temp1.up_parkingCode).p_Location_j][(int)pBL.GetParkingByCode(temp1.up_parkingCode).p_Location_i] = request;
                            return pBL.GetParkingByCode(temp1.up_parkingCode);
                        }
                        else
                        {
                            request.up_isUsing = true;
                            UpdateStatistics(request);
                            UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)pBL.GetParkingByCode
                                (temp1.up_parkingCode).p_Location_j][(int)pBL.GetParkingByCode(temp1.up_parkingCode).p_Location_i] = request;
                            return pBL.GetParkingByCode(temp1.up_parkingCode);
                        }
                    }
                    catch (Exception) { throw new IndexOutOfRangeException(); }
                }
                #endregion


                #region Optimization number 3: Unexpected
                foreach (BlockDTO block in bBL.GetAllBlocks().OrderByDescending(b => details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces))
                {
                    temp1 = null;
                    temp2 = null;
                    long a, b = 0, c, d = 0;
                    //left side
                    for (a = details[parkingLot.pl_code][block.b_code].leftFront; a < details[parkingLot.pl_code][block.b_code].leftBack; a++)
                    {
                        for (b = (int)(block.b_numberParkingsForJ / 2); b >= details[parkingLot.pl_code][block.b_code].currentLeftColumn + 2
                            && !UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)(b - 1)][(int)a].up_isUsing; b--) ;
                        //מחליפים עם שורה לפני השורה שבה הסמוך קטן או שווה לחניה להחלפה
                        if (request.up_leavingHour.TimeOfDay > UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                            [(int)(b - 1)][(int)a].up_leavingHour.TimeOfDay)
                        {
                            if (a > details[parkingLot.pl_code][block.b_code].leftFront)
                                a--;
                            break;
                        }
                    }
                    //If the found parking is later, and all the front parkings are earlier.
                    if (ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)b][(int)a].up_leavingHour.TimeOfDay
                            > request.up_leavingHour.TimeOfDay && UsedParkingsDict[parkingLot.pl_code][block.b_code]
                            .Last()[(int)b].GetRange((int)details[parkingLot.pl_code][block.b_code].leftFront,
                            (int)(a - details[parkingLot.pl_code][block.b_code].leftFront)).All(h => (h.up_isUsing &&
                            h.up_leavingHour.TimeOfDay <= request.up_leavingHour.TimeOfDay) || !pBL.GetParkingByCode(h.up_parkingCode).p_isLegal))
                    {
                        temp1 = ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)b][(int)a];
                    }

                    //right side
                    for (c = details[parkingLot.pl_code][block.b_code].rightFront; c < details[parkingLot.pl_code][block.b_code].rightBack; c++)
                    {
                        for (d = (int)(block.b_numberParkingsForJ / 2); d <= details[parkingLot.pl_code][block.b_code].currentRightColumn + 2
                            && !UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)(d + 1)][(int)a].up_isUsing; d++) ;
                        //מחליפים עם שורה לפני השורה שבה הסמוך קטן או שווה לחניה להחלפה
                        if (request.up_leavingHour.TimeOfDay > UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                            [(int)(d + 1)][(int)c].up_leavingHour.TimeOfDay)
                        {
                            if (c > details[parkingLot.pl_code][block.b_code].rightFront)
                                c--;
                            break;
                        }
                    }
                    //If the found parking is later, and all the front parkings are earlier.
                    if (ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)d][(int)c].up_leavingHour.TimeOfDay
                            > request.up_leavingHour.TimeOfDay && UsedParkingsDict[parkingLot.pl_code][block.b_code]
                            .Last()[(int)b].GetRange((int)details[parkingLot.pl_code][block.b_code].rightFront,
                            (int)(a - details[parkingLot.pl_code][block.b_code].rightFront)).All(h => (h.up_isUsing &&
                            h.up_leavingHour.TimeOfDay <= request.up_leavingHour.TimeOfDay) || !pBL.GetParkingByCode(h.up_parkingCode).p_isLegal))
                    {
                        temp2 = ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)c][(int)d];
                    }
                    try
                    {
                        //בדיקת ההפרשים, מי קטן יותר מבין שני הצדדים
                        if (UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)a][(int)b - 1].up_leavingHour.TimeOfDay
                               - temp1.up_leavingHour.TimeOfDay
                             <= UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)c][(int)d + 1].up_leavingHour.TimeOfDay
                               - temp2.up_leavingHour.TimeOfDay)
                        {
                            request.up_isUsing = true;
                            UpdateStatistics(request);
                            UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)b][(int)a] = request;
                            return pBL.GetParkingByCode(temp1.up_parkingCode);
                        }
                        else
                        {
                            request.up_isUsing = true;
                            UpdateStatistics(request);
                            UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)d][(int)a] = request;
                            return pBL.GetParkingByCode(temp2.up_parkingCode);
                        }
                    }
                    catch (Exception) { }
                }
                #endregion


                #region Optimization number 4: Routes

                foreach (BlockDTO block in bBL.GetAllBlocks().OrderByDescending(b => details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces))
                {
                    //left side
                    int i1;
                    for (i1 = (int)details[parkingLot.pl_code][block.b_code].leftBack;
                        i1 < (int)details[parkingLot.pl_code][block.b_code].leftFront &&
                        (UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn][i1].up_isUsing ||
                        pBL.GetParkingByCode(UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code]
                        .currentLeftColumn][i1].up_parkingCode).p_isLegal ||
                        (!UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn][i1].up_isUsing
                        && !UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn][i1].HasRoute(request)));
                        i1--) ;
                    if (UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn][i1].HasRoute(request))
                    {
                        request.up_isUsing = true;
                        UpdateStatistics(request);
                        UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn][i1] = request;
                        return pBL.GetParkingByCode(ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                            [(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn][i1].up_parkingCode);
                    }
                    //right side
                    for (i1 = (int)details[parkingLot.pl_code][block.b_code].rightBack;
                        i1 < (int)details[parkingLot.pl_code][block.b_code].rightFront &&
                        (UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentRightColumn][i1].up_isUsing ||
                        pBL.GetParkingByCode(UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code]
                        .currentRightColumn][i1].up_parkingCode).p_isLegal ||
                        (!UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentRightColumn][i1].up_isUsing
                        && !UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentRightColumn][i1].HasRoute(request)));
                        i1--) ;
                    if (UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentRightColumn][i1].HasRoute(request))
                    {
                        request.up_isUsing = true;
                        UpdateStatistics(request);
                        UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentRightColumn][i1] = request;
                        return pBL.GetParkingByCode(ResetHours.PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()
                            [(int)details[parkingLot.pl_code][block.b_code].currentRightColumn][i1].up_parkingCode);
                    }
                }
                #endregion


                #region Default: Blocking
                foreach (BlockDTO block in bBL.GetAllBlocks().OrderByDescending(b => details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces))
                {
                    AllFitParking1 = UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentLeftColumn]
                        .GetRange((int)details[parkingLot.pl_code][block.b_code].leftFront, (int)details[parkingLot.pl_code][block.b_code].leftBack
                        - (int)details[parkingLot.pl_code][block.b_code].leftFront).Count(c => c.up_isUsing)
                        < UsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)details[parkingLot.pl_code][block.b_code].currentRightColumn]
                        .GetRange((int)details[parkingLot.pl_code][block.b_code].rightFront, (int)details[parkingLot.pl_code][block.b_code].rightBack
                        - (int)details[parkingLot.pl_code][block.b_code].leftFront).Count(c => c.up_isUsing) ?
                        AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, ++details[parkingLot.pl_code][block.b_code].currentLeftColumn, request)
                        : AllFitParkingsInColumn(parkingLot.pl_code, block.b_code, --details[parkingLot.pl_code][block.b_code].currentRightColumn, request);
                    request.up_isUsing = true;
                    UpdateStatistics(request);
                    AllFitParking1[0] = request;
                    return pBL.GetParkingByCode(AllFitParking1[0].up_parkingCode);
                }
                #endregion


                if (status)
                {
                    ResetHours.Build(parkingLot);
                    FindParking(parkingLot, request, false);
                }
                return null;
            }
            return pBL.GetAllParkings()[0];
        }
    }

    public class Details
    {
        public long leftFront;
        public long leftBack;
        public long rightFront;
        public long rightBack;
        public long currentLeftColumn;
        public long currentRightColumn;
        public long numberOfEmptyPlaces;
    }
    public class Find
    {
        public ParkingLotDTO parkingLot { get; set; } = new ParkingLotDTO();
        public UsingParkingDTO request { get; set; } = new UsingParkingDTO();
        public bool status { get; set; } = true;
    }
}