using AutoMapper;
using EVChargingBackend.DTOs;
using EVChargingBackend.Models;
using System;

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
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now));

            CreateMap<UpdateBookingDto, Booking>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // User Mappings
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ChargingStation partial update mapping: ignore nulls so mapper only overwrites provided fields
            CreateMap<ChargingStationUpdateDto, ChargingStation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Optionally map ChargingStation to a DTO in future
        }
    }
}
