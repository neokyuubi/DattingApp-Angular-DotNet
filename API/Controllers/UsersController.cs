using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
		private readonly IUserRepository _userRepository;
		private readonly IUnitOfWork uw;
		private readonly IMapper _mapper;
		private readonly IPhotoService _photoService;

		public UsersController(IUnitOfWork uw, IMapper mapper, IPhotoService photoService)
        {
			this.uw = uw;
            _photoService = photoService;
			_mapper = mapper;
		}

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
			var username = User.GetUsername();
			var gender = await uw.UserRepository.GetUserGender(username);
			userParams.CurrentUsername = username;

			if (string.IsNullOrEmpty(userParams.Gender))
			{
				userParams.Gender = gender == "male" ? "female" : "male";
			}

            var users = await uw.UserRepository.GetMembersAsync(userParams);

			Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, 
			users.TotalCount, users.TotalPages));

			return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await uw.UserRepository.GetMemberAsync(username);
        }

		[HttpPut]
		public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
		{
			var user = await uw.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			if(user == null) return NotFound();
			
			_mapper.Map(memberUpdateDto, user);

			if (await uw.Complete()) return NoContent();

			return BadRequest("Failed to update user");
		}

		[HttpPost("add-photo")]
		public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
		{
			var user = await uw.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			if (user == null) return NotFound();

			var result = await _photoService.AddPhotoAsync(file);
			if (result.Error != null) return BadRequest(result.Error.Message);

			var photo = new Photo
			{
				Url = result.SecureUrl.AbsoluteUri,
				PublicId = result.PublicId
			};
			if (user.Photos.Count  == 0) photo.IsMain = true;

			user.Photos.Add(photo);

			if (await uw.Complete())
			{
				return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
			}
			return BadRequest("Problem adding photo");
		}


		[HttpPut("set-main-photo/{photoId}")]
		public async Task<ActionResult> SetMainPhoto(int photoId)
		{
			var user = await uw.UserRepository.GetUserByUsernameAsync(User.GetUsername());

			if(user == null) return NotFound();

			var photo = user.Photos.FirstOrDefault(photo=>photo.Id == photoId);

			if (photo == null) return NotFound();

			if(photo.IsMain) return BadRequest("this is already your main photo");

			var currentMain = user.Photos.FirstOrDefault(photo=>photo.IsMain);

			if (currentMain != null) currentMain.IsMain = false;
			photo.IsMain = true;

			if (await uw.Complete())
			{
				return NoContent();
			}
			else
			{
				return BadRequest("Problem setting the main photo");
			}

		}

		[HttpDelete("delete-photo/{photoId}")]
		public async Task<ActionResult> DeletePhoto(int photoId)
		{
			var user = await uw.UserRepository.GetUserByUsernameAsync(User.GetUsername());

			if(user == null) return NotFound();

			var photo = user.Photos.FirstOrDefault(photo=>photo.Id == photoId);

			if (photo == null) return NotFound();

			if(photo.IsMain) return BadRequest("You cannot delete the main photo");

			if (photo.PublicId != null)
			{
				var result = await _photoService.DeletePhotoAsync(photo.PublicId);
				if(result.Error != null) return BadRequest(result.Error.Message);
			}

			user.Photos.Remove(photo);

			if (await uw.Complete()) return Ok();
			
			return BadRequest("Problem deleting photo");
		}
    }
}