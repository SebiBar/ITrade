using ITrade.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITrade.DB
{
    public class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        public DbSet<SeedStatus> SeedStatuses { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<TokenType> TokenTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserProfileLink> UserProfileLinks { get; set; }
        public DbSet<UserProfileTag> UserProfileTags { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTag> ProjectTags { get; set; }
        public DbSet<RequestType> RequestTypes { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<ProjectStatusType> ProjectStatusTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SeedStatus>()
                .Property(x => x.ShouldSeedDatabase)
                .HasDefaultValue(true);

            //a project has a worker and an owner, users have owned and assigned projects
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId);
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Worker)
                .WithMany(u => u.AssignedProjects)
                .HasForeignKey(p => p.WorkerId);

            //a request has a sender and a receiver, users have sent and received requests
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.SentRequests)
                .HasForeignKey(r => r.SenderId);
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.ReceivedRequests)
                .HasForeignKey(r => r.ReceiverId);

            //a review has a reviewer and a reviewee, users have sent and received reviews
            modelBuilder.Entity<UserReview>()
                .HasOne(ur => ur.Reviewer)
                .WithMany(u => u.SentUserReviews)
                .HasForeignKey(ur => ur.ReviewerId);
            modelBuilder.Entity<UserReview>()
                .HasOne(ur => ur.Reviewee)
                .WithMany(u => u.ReceivedUserReviews)
                .HasForeignKey(ur => ur.RevieweeId);

            //auto filter soft deleted Projects
            modelBuilder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Request>().HasQueryFilter(r => !r.Project.IsDeleted);
            modelBuilder.Entity<ProjectTag>().HasQueryFilter(pt => !pt.Project.IsDeleted);

        }
    }
}
