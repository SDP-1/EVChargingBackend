using AutoMapper;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;

namespace EVChargingBackend.Mappings
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {

            // Booking Mappings
            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<CreateBookingDto, Booking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateBookingDto, Booking>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
