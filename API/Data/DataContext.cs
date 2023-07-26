using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }

		public DbSet<UserLike> Likes { get; set; }

		public DbSet<Message> Messages { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.Entity<UserLike>()
			.HasKey(key => new {key.SourceUserId, key.TargertUserId});

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