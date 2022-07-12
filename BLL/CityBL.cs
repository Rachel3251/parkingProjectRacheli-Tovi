using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using AutoMapper;

namespace BLL
{
    public class CityBL
    {

        public List<CityDTO> CitiesList = new List<CityDTO>();

        public CityBL()
        {

            RefreshCitiesList();
        }

        public void RefreshCitiesList()
        {
            CitiesList = new List<CityDTO>();
            using (PARKINGEntities ctx = new PARKINGEntities())
            {
                foreach (City city in ctx.Cities)
                {
                    CitiesList.Add(Mapper.Map<CityDTO>(city));
                }
            }
        }

        public long Insert(CityDTO city)
        {
            if (CitiesList.Find(c => c.c_name == city.c_name) == null)
                using (PARKINGEntities ctx = new PARKINGEntities())
                {
                    try
                    {
                        ctx.Cities.Add(Mapper.Map<City>(city));
                        ctx.SaveChanges();
                        RefreshCitiesList();
                        return CitiesList.First(u => u.c_code == city.c_code).c_code;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            return CitiesList.First(c => c.c_code == city.c_code).c_code;
        }

        public long Update(CityDTO city)
        {
            //if (CitiesList.Find(c => c.c_name == city.c_name) == null)
            //    using (PARKINGEntities ctx = new PARKINGEntities())
            //    {
            //        try
            //        {

            //            ctx.Cities.Add(Mapper.Map<City>(city));
            //            ctx.SaveChanges();
            //            RefreshCitiesList();
            //            return 0;
            //        }
            //        catch (Exception ex)
            //        {
            //            return 0;
            //        }
            //    }
            return 0;
        }

        public int Delete(CityDTO city)
        {
            if (CitiesList.Find(c => c.c_code == city.c_code) != null)
                using (PARKINGEntities ctx = new PARKINGEntities())
                {
                    try
                    {

                        ctx.Cities.Remove(Mapper.Map<City>(city));
                        ctx.SaveChanges();
                        RefreshCitiesList();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }


            return 0;

          
        }

        public List<CityDTO> GetAllCities()
        {
            return CitiesList;
        }

        public CityDTO GetCityByCode(long code)
        {
            return CitiesList.First(u => u.c_code == code);
        }
        public string GetCityNameByCode(long code)
        {
            return CitiesList.First(u => u.c_code == code).c_name;
        }
    }
}
