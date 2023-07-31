using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
	public class DataContext : IdentityDbContext<AppUser, AppRole, int,
	IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
	IdentityRoleClaim<int>, IdentityUserToken<int>>
	{
		public DataContext(DbContextOptions options) : base(options)
		{
		}


		public DbSet<UserLike> Likes { get; set; }

		public DbSet<Message> Messages { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<AppUser>()
			.HasMany(appUser => appUser.UserRoles)
			.WithOne(appUser => appUser.User)
			.HasForeignKey(appUser => appUser.UserId)
			.IsRequired();

			builder.Entity<AppRole>()
			.HasMany(appUser => appUser.UserRoles)
			.WithOne(appUser => appUser.Role)
			.HasForeignKey(appUser => appUser.RoleId)
			.IsRequired();

			builder.Entity<UserLike>()
			.HasKey(key => new { key.SourceUserId, key.TargertUserId });

			builder.Entity<UserLike>()
			.HasOne(source => source.SourceUser)
			.WithMany(target => target.LikedUsers)
			.HasForeignKey(foreign => foreign.SourceUserId)
			.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<UserLike>()
			.HasOne(source => source.TargetUser)
			.WithMany(target => target.LikedByUsers)
			.HasForeignKey(foreign => foreign.TargertUserId)
			.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<Message>()
			.HasOne(source => source.Recipient)
			.WithMany(target => target.MessagesReceived)
			.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Message>()
			.HasOne(source => source.Sender)
			.WithMany(target => target.MessagesSent)
			.OnDelete(DeleteBehavior.Restrict);

		}
	}
}