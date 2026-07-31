using LinkManagerPro.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkManagerPro.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<Click> Clicks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تحويل DateTime إلى timestamp with time zone
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Link>()
                .Property(l => l.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Link>()
                .Property(l => l.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Click>()
                .Property(c => c.ClickedAt)
                .HasColumnType("timestamp with time zone");

            // Foreign Key
            modelBuilder.Entity<Link>()
                .HasOne(l => l.User)
                .WithMany(u => u.Links)
                .HasForeignKey(l => l.UserId);
        }
    }
}
