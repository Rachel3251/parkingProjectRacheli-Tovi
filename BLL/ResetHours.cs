using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BLL
{
    public enum SIDES
    {
        left = 0,
        right = 1
    }

    public static class ResetHours
    {
        //דיקט עם תור הנכנסים, דיקט עם מטריצת רשימות שעות משוערות עבור כל בלוק

        //לגבי הסטטיסטיקה: המחלקה תירש מחונה קבוע על מנת שהרשימה תכיל את שני סוגי החונים
        //אין להסתמך על החונים הקבועים שיגיעו אלא רק על הסטטיסטיקה. החישוב פה הוא רק בינתיים

        public static string HourPath = @"..\..\..\TimeToRunTheSystemEveryDay.txt";
        public static string RunHour = File.ReadAllText(HourPath);
        public static string MinutesPath = @"..\..\..\WaitingTimeBetweenTwoUsers.txt";
        public static double MINUTES = Double.Parse(File.ReadAllText(MinutesPath));
        public const int FIVE = 5;
        public const int TWO = 2;
        public static DateTime day;
        public static BlockBL bBL = new BlockBL();
        public static ParkingBL pBL = new ParkingBL();
        public static PermanentUserBL puBL = new PermanentUserBL();
        public static ParkingLotBL plBL = new ParkingLotBL();
        public static UsingParkingBL upBL = new UsingParkingBL();
        public static List<BlockDTO> blocks;
        public static Dictionary<long, List<PermanentUserDTO>> HoursDict;
        //parkings of blocks of parking-lots - matrixes of predicated hours
        public static Dictionary<long, Dictionary<long, List<List<List<UsingParkingDTO>>>>> PredictedUsedParkingsDict;

        //בכל בוקר Build הפונקציה מזמנת את    
        //
        public static void Tick()
        {
            if (DateTime.Now.TimeOfDay.ToString() == RunHour)
            {
                //Rebuild every morning, for updating possible changements in the parking lots' structure
                //Rebuild also the matrix of FindParkingBL.
                blocks = bBL.GetAllBlocks();
                day = DateTime.Now;
                HoursDict = new Dictionary<long, List<PermanentUserDTO>>();
                PredictedUsedParkingsDict = new Dictionary<long, Dictionary<long, List<List<List<UsingParkingDTO>>>>>();
                FindParkingBL.UsedParkingsDict = new Dictionary<long, Dictionary<long, List<List<List<UsingParkingDTO>>>>>();
                FindParkingBL.details = new Dictionary<long, Dictionary<long, Details>>();
                foreach (ParkingLotDTO parkingLot in plBL.GetAllParkingLots())
                {
                    //The ones that fit the specific day and parking lot
                    HoursDict[parkingLot.pl_cityCode] = puBL.PermanentUsersList.Where(pu => pu.cu_parkingLotCode == parkingLot.pl_code
                            && pu.cu_dayCode == (int)(day.DayOfWeek + 1) && pu.cu_startDate.Date <= day.Date && pu.cu_lastDate.Date >= day.Date
                            && pu.cu_status == true).OrderBy(u => u.cu_entranceHour.TimeOfDay).ToList();
                    PredictedUsedParkingsDict[parkingLot.pl_code] = new Dictionary<long, List<List<List<UsingParkingDTO>>>>();
                    FindParkingBL.UsedParkingsDict[parkingLot.pl_code] = new Dictionary<long, List<List<List<UsingParkingDTO>>>>();
                    FindParkingBL.details[parkingLot.pl_code] = new Dictionary<long, Details>();
                    foreach (BlockDTO block in blocks.Where(p => p.b_parkingLotCode == parkingLot.pl_code))
                    {
                        PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code] = new List<List<List<UsingParkingDTO>>>();
                        FindParkingBL.UsedParkingsDict[parkingLot.pl_code][block.b_code] = new List<List<List<UsingParkingDTO>>>();
                        FindParkingBL.details[parkingLot.pl_code][block.b_code] = new Details();
                    }

                    Build(parkingLot);
                }
            }
        }


        //פונקציה המתבצעת עבור כל חניון: בזמן תחילת הפעילות, ובכל פעם שנגמר המקום בחניון מסוים
        //
        public static int Build(ParkingLotDTO parkingLot)
        {
            //sorts blocks
            List<BlockDTO> orderedBlocks = new List<BlockDTO>();
            long sumOfAllEmptyParkings = 0;
            foreach (BlockDTO block in blocks.Where(b => b.b_parkingLotCode == parkingLot.pl_code))
            {
                FindParkingBL.details[parkingLot.pl_code][block.b_code] = GetEmptyAreas(block);
                sumOfAllEmptyParkings += FindParkingBL.details[parkingLot.pl_code][block.b_code].numberOfEmptyPlaces;
            }

            if (sumOfAllEmptyParkings == 0)
            {
                return 1;
            }

            //starts with the biggest block, for optimizing the next fill
            orderedBlocks = blocks.Where(b => b.b_parkingLotCode == parkingLot.pl_code)
                    .OrderByDescending(b => FindParkingBL.details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces).ToList();

            //Removes the unnecessary hours that their enterance time had already passed.
            while (HoursDict[parkingLot.pl_code][0].cu_entranceHour.TimeOfDay > DateTime.Now.TimeOfDay)
            {
                HoursDict[parkingLot.pl_code].RemoveAt(0);
            }

            List<PermanentUserDTO> HoursForCurrentFilling;
            //fills the list 'HoursForCurrentFilling'
            if (HoursDict[parkingLot.pl_code].Count() >= sumOfAllEmptyParkings)
            {
                HoursForCurrentFilling = HoursDict[parkingLot.pl_code].GetRange(0, (int)sumOfAllEmptyParkings);
                HoursDict[parkingLot.pl_code].RemoveRange(0, (int)(sumOfAllEmptyParkings));
            }
            else //at the last time
            {
                HoursForCurrentFilling = HoursDict[parkingLot.pl_code];
                HoursDict[parkingLot.pl_code].Clear();
            }
            HoursForCurrentFilling.OrderBy(h => h.cu_leavingHour.TimeOfDay);

            foreach (BlockDTO block in orderedBlocks.Where(b => FindParkingBL.details[parkingLot.pl_code][b.b_code].numberOfEmptyPlaces > 0))
            {
                //Adding new matrixes
                List<List<UsingParkingDTO>> matrix;
                UsingParkingDTO copied;
                matrix = new List<List<UsingParkingDTO>>();//מוסיפים בסוף הפונקציה, לסוף הרשימה שלו.
                FindParkingBL.UsedParkingsDict[block.b_parkingLotCode][block.b_code].Add(new List<List<UsingParkingDTO>>());
                for (int j = 0; j < block.b_numberParkingsForJ; j++)
                {
                    matrix.Add(new List<UsingParkingDTO>());
                    FindParkingBL.UsedParkingsDict[block.b_parkingLotCode][block.b_code].Last().Add(new List<UsingParkingDTO>());
                    for (int i = 0; i < block.b_numberParkingsForI; i++)
                    {
                        matrix[j].Add(new UsingParkingDTO());
                        copied = FindParkingBL.UsedParkingsDict[block.b_parkingLotCode][block.b_code]
                                [(int)FindParkingBL.UsedParkingsDict[block.b_parkingLotCode][block.b_code].Count() - 2]
                                [j][i];
                        //if it is illegal or is used now
                        if (copied.up_isUsing || !pBL.GetAllParkings().FirstOrDefault(p => p.p_code == copied.up_parkingCode).p_isLegal)
                            FindParkingBL.UsedParkingsDict[block.b_parkingLotCode][block.b_code].Last()[j].Add(copied);
                        else
                            FindParkingBL.UsedParkingsDict[block.b_parkingLotCode][block.b_code].Last()[j].Add(new UsingParkingDTO());
                    }
                }

                List<PermanentUserDTO> HoursForBlock;
                int additionalParkings;
                Details temp;
                //for increasing the effectiveness, it checks again the empty places of each block, 
                //to consider parkings which were left in the meantime.
                //the first block won't fit the condition, because we already checked the first car in the HoursDict.
                temp = GetEmptyAreas(block);
                if (FindParkingBL.details[parkingLot.pl_code][block.b_code].leftFront != temp.leftFront ||
                        FindParkingBL.details[parkingLot.pl_code][block.b_code].leftBack != temp.leftBack ||
                        FindParkingBL.details[parkingLot.pl_code][block.b_code].rightFront != temp.rightBack ||
                        FindParkingBL.details[parkingLot.pl_code][block.b_code].rightBack == temp.rightBack)
                {
                    additionalParkings = (int)(temp.numberOfEmptyPlaces - FindParkingBL.details[parkingLot.pl_code][block.b_code].numberOfEmptyPlaces);
                    if (HoursDict[parkingLot.pl_code].Count() >= additionalParkings)
                    {
                        HoursForCurrentFilling.AddRange(HoursDict[parkingLot.pl_code].GetRange(0, additionalParkings));
                        HoursDict[parkingLot.pl_code].RemoveRange(0, additionalParkings);
                    }
                    else //for the last time
                    {
                        HoursForCurrentFilling.AddRange(HoursDict[parkingLot.pl_code]);
                        HoursDict[parkingLot.pl_code].Clear();
                    }
                    HoursForCurrentFilling.OrderBy(h => h.cu_leavingHour.TimeOfDay);
                    FindParkingBL.details[parkingLot.pl_code][block.b_code] = temp;
                }

                //fills the list 'HoursForBlock'
                if (HoursForCurrentFilling.Count() >= FindParkingBL.details[parkingLot.pl_code][block.b_code].numberOfEmptyPlaces)
                {
                    HoursForBlock = HoursForCurrentFilling.GetRange(0, (int)FindParkingBL.details[parkingLot.pl_code][block.b_code].numberOfEmptyPlaces);
                    HoursForCurrentFilling.RemoveRange(0, (int)FindParkingBL.details[parkingLot.pl_code][block.b_code].numberOfEmptyPlaces);
                }
                else //for the last time
                {
                    HoursForBlock = HoursForCurrentFilling;
                    HoursForCurrentFilling.Clear();
                }
                HoursForBlock.OrderBy(a => a.cu_entranceHour);

                //checks all the parkings in the range. if one of them is illegal, the loop moves to the other side.
                for (long j = 0; j < (block.b_numberParkingsForJ) / 2 && HoursForBlock != null; j++)
                {
                    for (long l = FindParkingBL.details[parkingLot.pl_code][block.b_code].leftFront,
                              r = FindParkingBL.details[parkingLot.pl_code][block.b_code].rightFront;
                             (l < FindParkingBL.details[parkingLot.pl_code][block.b_code].leftBack ||/* V */
                              r < FindParkingBL.details[parkingLot.pl_code][block.b_code].rightBack) && HoursForBlock != null;
                              l++, r++)
                    {

                        //left side
                        if (ParkingLotBL.ParkingLotsDict[parkingLot.pl_code][block.b_code][l][j].p_isLegal
                                && l < FindParkingBL.details[parkingLot.pl_code][block.b_code].leftBack)
                        {
                            PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)j][(int)l] = new UsingParkingDTO()
                            {
                                up_parkingCode = ParkingLotBL.ParkingLotsDict[parkingLot.pl_code][block.b_code][l][j].p_code,
                                up_isUsing = false,
                                up_entranceHour = HoursForBlock[0].cu_entranceHour,
                                up_leavingHour = HoursForBlock[0].cu_leavingHour,
                                up_date = day.Date
                            };
                            HoursForBlock.RemoveAt(0);
                        }

                        //right side
                        if (ParkingLotBL.ParkingLotsDict[parkingLot.pl_code][block.b_code][r][block.b_numberParkingsForJ - 1 - j].p_isLegal
                                && r < FindParkingBL.details[parkingLot.pl_code][block.b_code].rightBack && HoursForBlock != null)
                        {
                            PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)(block.b_numberParkingsForJ - 1 - j)][(int)r] = new UsingParkingDTO()
                            {
                                up_parkingCode = ParkingLotBL.ParkingLotsDict[parkingLot.pl_code][block.b_code][r][block.b_numberParkingsForJ - 1 - j].p_code,
                                up_isUsing = false,
                                up_entranceHour = HoursForBlock[0].cu_entranceHour,
                                up_leavingHour = HoursForBlock[0].cu_leavingHour,
                                up_date = day.Date
                            };
                            HoursForBlock.RemoveAt(0);
                        }
                        PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)j].OrderBy(p => p.up_leavingHour);
                        PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Last()[(int)(block.b_numberParkingsForJ - 1 - j)].OrderBy(p => p.up_leavingHour);
                    }

                }
                if (HoursForCurrentFilling == null) break;
                PredictedUsedParkingsDict[parkingLot.pl_code][block.b_code].Add(matrix);
            }
            return 0;
        }


        //פונקצית עזר למציאת רצף השורות הגדול ביותר בשני חצאי הבלוק שהתקבל
        //הפונקציה מחזירה פרטים אודות התנהגות הבלוק עבור מילוי זה


        //לתקן את הדיטיילס
        public static Details GetEmptyAreas(BlockDTO block)
        {
            Details details = new Details()
            {
                leftFront = -1,
                leftBack = -1,
                rightFront = -1,
                rightBack = -1,
                numberOfEmptyPlaces = 0,
                currentLeftColumn = 0,
                currentRightColumn = 0
            };

            int sumRows, sumOfEmptyParkings, temp = 0;
            //left side
            for (int i = 0; i < block.b_numberParkingsForI; i++)
            {
                sumRows = 0;
                sumOfEmptyParkings = 0;
                //Counts empty rows
                while (CreateRowList(block.b_parkingLotCode, block.b_code, i, SIDES.left).All(up => up.up_isUsing == false ||
                        up == null || pBL.GetParkingByCode(up.up_parkingCode).p_isLegal == false)) // had left / empty / illegal
                {
                    sumRows++;
                    i++;
                }
                if ((sumRows >= FIVE - TWO && i == sumRows/*first*/) || (sumRows >= FIVE/*middle*/) ||
                       (sumRows >= FIVE - TWO && i == block.b_numberParkingsForI/*last*/))
                {
                    //Counts empty parkings
                    for (int k = i + 1 - sumRows; k < i; k++)
                    {
                        sumOfEmptyParkings += CreateRowList(block.b_parkingLotCode, block.b_code, k, SIDES.left).Where(
                                up => pBL.GetParkingByCode(up.up_parkingCode).p_isLegal || up == null).Count();
                    }
                }
                // Condition '>' and not '>=' , in purpose of preferring the front area.
                if (sumOfEmptyParkings > details.numberOfEmptyPlaces)
                {
                    details.leftFront = i + 1 - sumRows;
                    //For keeping the path empty:
                    details.leftBack = i - 3;
                    temp = sumOfEmptyParkings;
                }
            }

            //right side
            for (int i = 0; i < block.b_numberParkingsForI; i++)
            {
                sumRows = 0;
                sumOfEmptyParkings = 0;
                //Counts empty rows
                while (CreateRowList(block.b_parkingLotCode, block.b_code, i, SIDES.right).All(up => up.up_isUsing == false ||
                        up == null || pBL.GetParkingByCode(up.up_parkingCode).p_isLegal == false)) // had left / empty / illegal
                {
                    sumRows++;
                    i++;
                }
                if ((sumRows >= FIVE - TWO && i == sumRows/*first*/) || (sumRows >= FIVE/*middle*/) ||
                       (sumRows >= FIVE - TWO && i == (block.b_numberParkingsForI)/*last*/))
                {
                    //Counts empty parkings
                    for (int k = i + 1 - sumRows; k < i; k++)
                    {
                        sumOfEmptyParkings += CreateRowList(block.b_parkingLotCode, block.b_code, k, SIDES.right).Where(
                                up => pBL.GetParkingByCode(up.up_parkingCode).p_isLegal || up == null).Count();
                    }
                }
                if (sumOfEmptyParkings > details.numberOfEmptyPlaces)
                {
                    details.rightFront = i + 1 - sumRows;
                    //For keeping the path empty:
                    details.rightBack = i - 3;
                    temp += sumOfEmptyParkings;
                }
            }
            details.numberOfEmptyPlaces = temp;
            return details;
        }


        // right / left פונקצית עזר לבניית רשימה משורה נתונה, עבור שימוש בפונקציות ליסט בעת הצורך. הרשימה מכילה חצי שורה בלבד, ע"פ פרמטר של  
        //כאן הסתמכתי על כך שאם מספר העמודות הוא זוגי, החצי הגדול באחד נמצא בצד שמאל
        //חשוב מאוד עבור בניית החניון
        public static List<UsingParkingDTO> CreateRowList(long parkingLotCode, long blockCode, int i, SIDES s)
        {
            List<UsingParkingDTO> list = new List<UsingParkingDTO>();
            switch (s)
            {
                case SIDES.left:
                    for (long j = 0; j < blocks.Where(b => b.b_code == blockCode).FirstOrDefault().b_numberParkingsForJ / 2 - 1; j++)
                    {
                        list.Add(PredictedUsedParkingsDict[parkingLotCode][blockCode].Last()[(int)j][i]);
                    }
                    break;
                case SIDES.right:
                    for (long j = blocks.Where(b => b.b_code == blockCode).FirstOrDefault().b_numberParkingsForJ / 2 + 1;
                                j < blocks.Where(b => b.b_code == blockCode).FirstOrDefault().b_numberParkingsForJ - 1; j++)
                    {
                        list.Add(PredictedUsedParkingsDict[parkingLotCode][blockCode].Last()[(int)j][i]);
                    }
                    break;
                default: break;
            }
            return list;
        }

    }
}
