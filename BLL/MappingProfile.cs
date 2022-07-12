using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DAL;
using Models;
namespace BLL
{
    class MappingProfile:Profile
    {
        public MappingProfile()
        {

            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
            CreateMap<City, CityDTO>();
            CreateMap<CityDTO, City>();
            CreateMap<UsingParking, UsingParkingDTO>();
            CreateMap<UsingParkingDTO, UsingParking>();
            CreateMap<UserPermission, UserPermissionDTO>();
            CreateMap<UserPermissionDTO, UserPermission>();
            CreateMap<PermanentUser, PermanentUserDTO>();
            CreateMap<PermanentUserDTO, PermanentUser>();
            CreateMap<ParkingDTO, Parking>();
            CreateMap<Parking, ParkingDTO>();


        }
    }
}
