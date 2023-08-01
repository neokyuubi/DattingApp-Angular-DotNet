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

			CreateMap<MemberUpdateDto, AppUser>();
			CreateMap<RegisterDto, AppUser>();

			CreateMap<Message, MessageDto>()
			.ForMember(message => message.SenderPhotoUrl, 
			options => options.MapFrom(source=>source.Sender.Photos.FirstOrDefault(photo=>photo.IsMain).Url))
			.ForMember(message => message.RecipientPhotoUrl, 
			options => options.MapFrom(source=>source.Recipient.Photos.FirstOrDefault(photo=>photo.IsMain).Url));

			CreateMap<DateTime, DateTime>().ConvertUsing(datetime => DateTime.SpecifyKind(datetime, DateTimeKind.Utc));
			CreateMap<DateTime?, DateTime?>().ConvertUsing(datetime => datetime.HasValue ? DateTime.SpecifyKind(datetime.Value, DateTimeKind.Utc) : null);
		}
    }
}