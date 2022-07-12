using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Models;


namespace BLL
{
    public enum Directions
    {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3
    }
    public enum Status
    {
        PARKING = 0,
        ENTERANCE = 1,
        EXIT = 2,
        PASS = 3,
        ILLEGAL = 4
    }


    public static class BuildParkingLotBL
    {
        static string sizesPath = @"C:\Users\IMOE001\Desktop\פרויקט\Parking\BLL\Files\ParkingSizes.txt";
        static BlockBL bBL = new BlockBL();
        //מציאת מספר החניות שנכנסות בשטח שהתקבל
        //
        public static int[] NumberOfParkings(double height, double width)
        {
            string[] sizes = File.ReadAllText(sizesPath).Split(' ');
            int[] numOfParkings = new int[] { (int)(height / double.Parse(sizes[2])), (int)(width / double.Parse(sizes[5])) };
            return numOfParkings;
        }

        //מוסיפה מעברים, אם הכניסה או היציאה אמורות להוות מעבר, היא הופכת את המיקום גם ללא חוקי.
        //וכן כל מעבר הופך גם ללא חוקי! 
        //כי כל פעם שנשנה משהו נבדוק מחדש וגם נוסיף מעברים מחדש.
        public static void AddPasses(this List<ParkingDTO> parkingsOfBlock, BlockDTO block)
        {
            ParkingDTO[][] mat = new ParkingDTO[block.b_numberParkingsForI][];
            for (int i = 0; i < block.b_numberParkingsForI; i++)
            {
                mat[i] = new ParkingDTO[block.b_numberParkingsForJ];
                //worse performance
                //for (int j = 0; j < block.b_numberParkingsForJ; j++)
                //{
                //    mat[i][j] = parkingsOfBlock.First(p => p.Location_i == i && p.Location_j == j);
                //}
            }
            foreach (ParkingDTO parking in parkingsOfBlock)
            {
                mat[parking.p_Location_i][parking.p_Location_j] = parking;
            }
            foreach (ParkingDTO parking in mat[block.b_numberParkingsForJ / 2].Where(p => p.p_isLegal))
            {
                mat.EnterPasses(parking.p_Location_i, parking.p_Location_j);
            }
            for (long i = block.b_numberParkingsForI - 1; i >= 0; i--)
            {
                for (long j = 0; j < block.b_numberParkingsForJ; j++)
                {
                    switch (mat[i][j].p_status)
                    {
                        case "ILLEGAL":
                            mat.EnterPasses(i, j + 1);
                            mat.EnterPasses(i - 1, j + 1);
                            mat.EnterPasses(i - 1, j);
                            mat.EnterPasses(i - 1, j - 1);
                            mat.EnterPasses(i, j - 1);
                            mat.EnterPasses(i + 1, j - 1);
                            if (j > 0 && j < block.b_numberParkingsForJ - 2)
                                if (mat[i][j - 1].p_status != "PARKING" || mat[i][j + 1].p_status != "PARKING")
                                    mat.EnterPasses(i + 1, j);
                            mat.EnterPasses(i + 1, j + 1);
                            //Double pass for the sides.
                            if (mat[i][j - 1].p_status == "ILLEGAL" && mat[i][j + 1].p_status == "PASS")
                            {
                                mat.EnterPasses(i - 1, j + 2);
                                mat.EnterPasses(i, j + 2);
                                mat.EnterPasses(i + 1, j + 2);
                            }
                            if (mat[i][j + 1].p_status == "ILLEGAL" && mat[i][j - 1].p_status == "PASS")
                            {
                                mat.EnterPasses(i - 1, j - 2);
                                mat.EnterPasses(i, j - 2);
                                mat.EnterPasses(i + 1, j - 2);
                            }
                            break;
                        default: break;
                    }
                }
            }
            for (long j = 0; j <= block.b_numberParkingsForJ / 2; j++)
            {
                //exit - left
                try
                {
                    if (mat[0][(block.b_numberParkingsForJ / 2) - 1 - j].p_status != "EXIT")
                        if (mat[0][(block.b_numberParkingsForJ / 2) - 2 - j].p_status == "EXIT")
                        {
                            mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) - 1 - j);
                            mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) - 2 - j);
                        }
                        else
                        {
                            mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) - 1 - j);
                            mat.EnterPasses(1, (block.b_numberParkingsForJ / 2) - 1 - j);
                            mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) - 2 - j);
                            mat.EnterPasses(1, (block.b_numberParkingsForJ / 2) - 2 - j);
                        }
                }
                //The corner
                catch (Exception)
                {
                    long k = 0;
                    while (mat[0][k].p_status != "EXIT") k++;
                    mat.EnterPasses(0, k);
                    if (k > 1) mat.EnterPasses(1, k);
                }

                if (j < block.b_numberParkingsForJ) //sometimes the right side is smaller.
                {
                    //exit - right
                    try
                    {
                        if (mat[0][(block.b_numberParkingsForJ / 2) + 1 + j].p_status != "EXIT")
                            if (mat[0][(block.b_numberParkingsForJ / 2) + 2 + j].p_status == "EXIT")
                            {
                                mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) + 1 + j);
                                mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) + 2 + j);
                            }
                            else
                            {
                                mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) + 1 + j);
                                mat.EnterPasses(1, (block.b_numberParkingsForJ / 2) + 1 + j);
                                mat.EnterPasses(0, (block.b_numberParkingsForJ / 2) + 2 + j);
                                mat.EnterPasses(1, (block.b_numberParkingsForJ / 2) + 2 + j);
                            }
                    }
                    //The corner
                    catch (Exception)
                    {
                        long k = block.b_numberParkingsForJ - 1;
                        while (mat[0][k].p_status != "EXIT") k--;
                        mat.EnterPasses(0, k);
                        if (k < block.b_numberParkingsForJ - 2)
                            mat.EnterPasses(1, k);
                    }
                }
            }

            Directions blockDirection = Directions.SOUTH;
            for (int i = 0; i < block.b_numberParkingsForI - 1 && parkingsOfBlock.Any(p => p.p_status == "ENTERANCE" && p.p_Location_j == 0); i++)
            {
                mat.EnterPasses(i, 0);
                mat.EnterPasses(i, 1);
                blockDirection = Directions.WEST;
            }
            for (int i = 0; i < block.b_numberParkingsForI - 1 && parkingsOfBlock.Any(p => p.p_status == "ENTERANCE" && p.p_Location_j == block.b_numberParkingsForJ - 1); i++)
            {
                mat.EnterPasses(i, block.b_numberParkingsForJ - 1);
                mat.EnterPasses(i, block.b_numberParkingsForJ - 2);
                blockDirection = Directions.EAST;
            }
            switch (blockDirection)
            {
                case Directions.SOUTH:
                    for (long j = 0; j <= block.b_numberParkingsForJ / 2; j++)
                    {
                        //enterance - left
                        try
                        {
                            if (mat[block.b_numberParkingsForI - 1][(block.b_numberParkingsForJ / 2) - 1 - j].p_status != "ENTERANCE")
                                if (mat[block.b_numberParkingsForI - 1][(block.b_numberParkingsForJ / 2) - 2 - j].p_status == "ENTERANCE")
                                {
                                    mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) - 1 - j);
                                    mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) - 2 - j);
                                }
                                else
                                {
                                    mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) - 1 - j);
                                    mat.EnterPasses(block.b_numberParkingsForI - 2, (block.b_numberParkingsForJ / 2) - 1 - j);
                                    mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) - 2 - j);
                                    mat.EnterPasses(block.b_numberParkingsForI - 2, (block.b_numberParkingsForJ / 2) - 2 - j);
                                }
                        }
                        //The corner
                        catch (Exception)
                        {
                            long k = 0;
                            while (mat[block.b_numberParkingsForI - 1][k].p_status != "ENTERANCE") k++;
                            mat.EnterPasses(block.b_numberParkingsForI - 1, k);
                            if (k > 1) mat.EnterPasses(block.b_numberParkingsForI - 2, k);
                        }
                        if (j < block.b_numberParkingsForJ) //sometimes the right side is smaller, when the number of the columns is ZUGI.
                        {
                            //enterance - right
                            try
                            {
                                if (mat[block.b_numberParkingsForI - 1][(block.b_numberParkingsForJ / 2) + 1 + j].p_status != "ENTERANCE")
                                    if (mat[block.b_numberParkingsForI - 1][(block.b_numberParkingsForJ / 2) + 2 + j].p_status == "ENTERANCE")
                                    {
                                        mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) + 1 + j);
                                        mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) + 2 + j);
                                    }
                                    else
                                    {
                                        mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) + 1 + j);
                                        mat.EnterPasses(block.b_numberParkingsForI - 2, (block.b_numberParkingsForJ / 2) + 1 + j);
                                        mat.EnterPasses(block.b_numberParkingsForI - 1, (block.b_numberParkingsForJ / 2) + 2 + j);
                                        mat.EnterPasses(block.b_numberParkingsForI - 2, (block.b_numberParkingsForJ / 2) + 2 + j);
                                    }
                            }
                            //אם הגענו לפינה ואין יציאה או כניסה חוזרים אחורה או קדימה ומסמנים את היציאה כמעבר. כל הדרך היא בכל מקרה מעבר כפול
                            //The corner
                            catch (Exception)
                            {
                                long k = block.b_numberParkingsForJ - 1;
                                while (mat[block.b_numberParkingsForI - 1][k].p_status != "ENTERANCE") k--;
                                mat.EnterPasses(block.b_numberParkingsForI - 1, k);
                                if (k < block.b_numberParkingsForJ - 2)
                                    mat.EnterPasses(block.b_numberParkingsForI - 2, k);
                            }
                        }
                    }

                    break;
                default:
                    for (int j = 0; j < block.b_numberParkingsForJ - 1; j++)
                    {
                        mat.EnterPasses(block.b_numberParkingsForI - 1, j);
                        mat.EnterPasses(block.b_numberParkingsForI - 2, j);
                    }
                    break;
            }

            parkingsOfBlock = null;
            block.b_legalParkings = 0;
            for (int i = 0; i < mat.Length; i++)
            {
                for (int j = 0; j < mat[i].Length; j++)
                {
                    if (mat[i][j].p_isLegal)
                        block.b_legalParkings++;
                    parkingsOfBlock.Add(mat[i][j]);
                }
            }
            block.b_enteranceDirection = blockDirection.ToString();

        }

        public static void NamingParkings(this List<ParkingDTO> parkingsOfBlock)
        {
            ParkingDTO[][] mat = new ParkingDTO[bBL.GetBlockByCode(parkingsOfBlock[0].p_blockNumber).b_numberParkingsForI][];
            for (int i = 0; i < mat.Length; i++)
            {
                mat[i] = new ParkingDTO[bBL.GetBlockByCode(parkingsOfBlock[0].p_blockNumber).b_numberParkingsForJ];
                //worse performance
                //for (int j = 0; j < block.b_numberParkingsForJ; j++)
                //{
                //    mat[i][j] = parkingsOfBlock.First(p => p.Location_i == i && p.Location_j == j);
                //}
            }
            foreach (ParkingDTO parking in parkingsOfBlock)
            {
                mat[parking.p_Location_i][parking.p_Location_j] = parking;
            }
            string t;
            for (int i = 0; i < mat.Length; i++)
            {
                for (int j = 0; j < mat[0].Length; j++)
                {
                    if (i >= 26)
                        t = Char.ConvertFromUtf32('A' + i - 26) + "" + Char.ConvertFromUtf32('A' + i - 26);
                    else
                    {
                        t = Char.ConvertFromUtf32('A' + i);
                    }
                    mat[i][j].p_name = (t + "" + (1 + j)).ToString();
                }
            }
        }



        //חחחחחחחחחחחחחחחחחחחשששששששששששששששששוווווווווווווווווווווובבבבבבבבבבבבבבבבבבבבבבבבבבבב
        //לעיתים לאחר החלוקה לבלוקים, יש לבדוק אם יש כניסה ויציאה לכל בלוק, או מעברים המובילים לכניסה ויציאה
        //איך נדע לאיזה כיוון יהיה כל בלוק, כדי להגדיר את מספר החניות שנכנסות בשטח? הרי אין לי עדיין את כיווני הכניסה והיציאה של כל בלוק, ומצד שני - אני  צריכה לחלק את השטח לחניות כדי לסמן מיקומים לא חוקיים! מה יהיה??????.

        //מציאת כיווני כניסה ויציאה ושמירת המטריצה על פי הכיוון
        //בנתינת עדיפות לכניסה ויציאה נגדיות שתופסות פחות מעברים, אח"כ צמודות ואח"כ באותו צד
        //על המטריצה לחזור עם כיוון כניסה כלפי מעלה. מיקום הכניסה הממשי יוחזר כפרמטר

        //בהוספת מעברים למיקומים לא חוקיים, יש לבדוק אם זהו מקום צדדי שאינו חוסם, ואם כך - אין צורך במעבר(חניון בצורת עיגול, לדוגמה)אוקי

        //סיבוב המטריצה
        //מציאת מעברים
        //



        //public static bool CheckParkingLotFitting(List<ParkingDTO> list/*List<BlockDTO> blocks, Dictionary<long, ParkingDTO[][]> parkingsOfBlocks*/)
        //{
        //ParkingDTO[][] mat;
        //int sumOfParkings = 0;
        //int sumOfPasses = 0;

        //foreach (BlockDTO block in blocks)
        //{
        //    mat = parkingsOfBlocks[block.b_code];
        //    Directions direction = mat.ChangeDirection(block.b_numberParkingsForI, block.b_numberParkingsForJ);
        //    for (int i = 0; i < block.b_numberParkingsForJ; i++) //Enterance pass
        //    {
        //        if (mat[0][i].p_status != "ENTERANCE")
        //        {
        //            mat[1][i].p_status = "PASS";
        //            sumOfPasses++;
        //        }
        //    }
        //    for (int i = 0; i < block.b_numberParkingsForJ; i++) //Exit pass
        //    {
        //        if (mat[block.b_numberParkingsForI - 1][i].p_status != "EXIT")
        //        {
        //            mat[block.b_numberParkingsForI - 2][i].p_status = "PASS";
        //            sumOfPasses++;
        //        }
        //    }
        //    for (int i = 0; i < block.b_numberParkingsForI; i++) //Middle pass
        //    {
        //        if (mat[i][block.b_numberParkingsForJ / 2].p_status == "PARKING")
        //        {
        //            mat[i][block.b_numberParkingsForJ / 2].p_status = "PASS";
        //            sumOfPasses++;
        //        }
        //    }
        //    //להמשיך מכאן

        //    //אלגוריתם למציאת גודל השטח הלא-חוקי, וסימון המעברים הנדרשים עבורו
        //    //?איך יודעים מאיזה כיוון להגדיר מעבר לעמוד
        //    //המעבר יהיה לכיוון העמודה האמצעית, כיול שאין גישה מבפנים, שלושה מעברים עבור כל עמוד 
        //    //אלו שמאחוריו ולפניו יכולים להשתלב בנתיב
        //    //חשוב!!!!!!!!!!!!!!  לזכור לבדוק שמילוי חוזר לא נפגע בעקבות עמודים


        //}

        ////mat.Count(mat.Where(p=>p.p_status == "PARKING"))

        //list.AddPasses(block);
        //return list.Where(p => !p.isLegal).Count() <= C * (block.numberParkingsForI * block.numberParkingsForJ);


        //   }

        public static void EnterPasses(this ParkingDTO[][] mat, long i, long j)
        {
            try
            {
                if (mat[i][j].p_isLegal)
                {
                    mat[i][j].p_isLegal = false;
                    if (mat[i][j].p_status == "PARKING")
                        mat[i][j].p_status = "PASS";
                }
            }
            catch (Exception) { }
        }

    }
}
