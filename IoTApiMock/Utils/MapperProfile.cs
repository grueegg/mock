using AutoMapper;
using IoTApiMock.Models;

namespace IoTApiMock.DTO
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<DeviceModel, DeviceDto>();
            CreateMap<DeviceDto, DeviceModel>();
        }
    }
}
