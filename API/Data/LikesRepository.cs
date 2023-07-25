using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
	public class LikesRepository : ILikesRepository
	{
		private readonly DataContext _context;

		public LikesRepository(DataContext context)
		{
			_context = context;
		}

	
		public async Task<UserLike> GetUserLike(int SourceUserId, int TargertUserId)
		{
			return await _context.Likes.FindAsync(SourceUserId, TargertUserId);
		}

		public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
		{
			var users = _context.Users.OrderBy(user =>user.UserName).AsQueryable();
			var likes = _context.Likes.AsQueryable();

			if (likesParams.Predicate == "liked")
			{
				likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
				users = likes.Select(like => like.TargetUser);
			}

			if (likesParams.Predicate == "likedBy")
			{
				likes = likes.Where(like => like.TargertUserId == likesParams.UserId);
				users = likes.Select(like => like.SourceUser);
			}

			var likedUsers = users.Select(user => new LikeDto
			{
				UserName = user.UserName,
				KnownAs = user.KnownAs,
				Age = user.DateOfBirth.CalculateAge(),
				PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain).Url,
				City = user.City,
				Id = user.Id
			});

			return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
		}

		public async Task<AppUser> GetUserWithLikes(int userId)
		{
			return await _context.Users.Include(user => user.LikedUsers)
			.FirstOrDefaultAsync(user => user.Id == userId);
		}
	}
}