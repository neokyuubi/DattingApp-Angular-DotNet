using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
		private readonly IMapper _mapper;

		public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
			_context = context;
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



            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
				KnownAs = user.KnownAs,
				Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.Include(photo => photo.Photos).SingleOrDefaultAsync<AppUser>(user => user.UserName == loginDto.UserName);
            if (user == null) return Unauthorized("invalid username");
            
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
				PhotoUrl = user.Photos.FirstOrDefault(photo=>photo.IsMain)?.Url,
				KnownAs = user.KnownAs,
				Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync<AppUser>(user => user.UserName == username.ToLower());
        }
    }
}