using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
		{
			CreateMap<AppUser, MemberDto>()
			.ForMember(destinationMember => destinationMember.PhotoUrl, 
			options => options.MapFrom(source=>source.Photos.FirstOrDefault(photo=>photo.IsMain).Url))
			.ForMember(destinationMember => destinationMember.Age, options => options.MapFrom(source=> source.DateOfBirth.CalculateAge()));

			CreateMap<Photo, PhotoDto>();
		}
    }
}