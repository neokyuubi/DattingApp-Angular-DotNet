using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class LikesController : BaseApiController
	{
		private readonly IUnitOfWork uw;

		public LikesController(IUnitOfWork uw)
		{
			this.uw = uw;
		}

		[HttpPost("{username}")]
		public async Task<ActionResult> AddLikes(string username)
		{
			var sourceUserId = User.GetUserId();
			var likedUser = await uw.UserRepository.GetUserByUsernameAsync(username);
			var sourceUser = await uw.LikesRepository.GetUserWithLikes(sourceUserId);

			if (likedUser == null) return NotFound();
			if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

			var userLike = await uw.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);
			if (userLike != null) return BadRequest("You aready like this user");

			userLike = new UserLike
			{
				SourceUserId = sourceUserId,
				TargertUserId = likedUser.Id
			};

			sourceUser.LikedUsers.Add(userLike);
			
			if(await uw.Complete()) return Ok();

			return BadRequest("Failed to like user");
		}

		[HttpGet]
		public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
		{
			likesParams.UserId = User.GetUserId();
			var users = await uw.LikesRepository.GetUserLikes(likesParams);
			Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
			return Ok(users);
		}
	}
}