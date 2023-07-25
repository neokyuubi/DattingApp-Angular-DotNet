using API.DTOs;
using API.Entities;
using API.Extensions;
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

		public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
		{
			var users = _context.Users.OrderBy(user =>user.UserName).AsQueryable();
			var likes = _context.Likes.AsQueryable();

			if (predicate == "liked")
			{
				likes = likes.Where(like => like.SourceUserId == userId);
				users = likes.Select(like => like.TargetUser);
			}

			if (predicate == "likedBy")
			{
				likes = likes.Where(like => like.TargertUserId == userId);
				users = likes.Select(like => like.SourceUser);
			}

			return await users.Select(user => new LikeDto
			{
				UserName = user.UserName,
				KnownAs = user.KnownAs,
				Age = user.DateOfBirth.CalculateAge(),
				PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain).Url,
				City = user.City,
				Id = user.Id
			})
			.ToListAsync();
		}

		public async Task<AppUser> GetUserWithLikes(int userId)
		{
			return await _context.Users.Include(user => user.LikedUsers)
			.FirstOrDefaultAsync(user => user.Id == userId);
		}
	}
}