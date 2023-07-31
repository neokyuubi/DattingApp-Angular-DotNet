using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
		private readonly UserManager<AppUser> _userManager;
		private readonly ITokenService _tokenService;
		private readonly IMapper _mapper;

		public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
			_userManager = userManager;
			_tokenService = tokenService;
        }

        [HttpPost("register")] // api/account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is taken");
            }

			var user = _mapper.Map<AppUser>(registerDto); // get AppUser from RegisterDto

			user.UserName = registerDto.UserName.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

			if(!result.Succeeded) return BadRequest(result.Errors);

			var roleResult = await _userManager.AddToRoleAsync(user, "Member");

			if(!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
				KnownAs = user.KnownAs,
				Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(appUser => appUser.Photos).SingleOrDefaultAsync(user => user.UserName == loginDto.UserName);
            if (user == null) return Unauthorized("invalid username"); // TODO :: Unauthorized

			var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

			if(!result) return Unauthorized("invalid password"); // TODO :: Unauthorized
            
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
				PhotoUrl = user.Photos.FirstOrDefault(photo=>photo.IsMain)?.Url,
				KnownAs = user.KnownAs,
				Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync<AppUser>(user => user.UserName == username.ToLower());
        }
    }
}