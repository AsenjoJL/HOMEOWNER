using HOMEOWNER.Models;
using Microsoft.EntityFrameworkCore;

namespace HOMEOWNER.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Homeowner> Homeowners { get; set; }
        public DbSet<HomeownerProfileImage> HomeownerProfileImages { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        public DbSet<Announcement> Announcements { get; set; }

        public DbSet<CommunitySettings> CommunitySettings { get; set; }


        public DbSet<Reaction> Reactions { get; set; } // Add this line


        public DbSet<ForumPost> ForumPosts { get; set; }

        public DbSet<ForumComment> ForumComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ForumComment>()
                .HasOne(fc => fc.Homeowner)
                .WithMany()
                .HasForeignKey(fc => fc.HomeownerID)
                .OnDelete(DeleteBehavior.NoAction); // Avoids cascade path issues
        }


    }
}
