using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace BLL
{
    public class AutoMapperConfig
    {
        public void InitializeMap()
        {
            Mapper.Initialize(cfg => {
                cfg.AddProfile<MappingProfile>();
            });
        }
    }
}
