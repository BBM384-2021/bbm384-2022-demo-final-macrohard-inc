using Microsoft.EntityFrameworkCore;
using LinkedHU_CENG.Models;
namespace LinkedHU_CENG.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Follow>()
                .HasOne<Account>(f => f.Account1)
                .WithMany(a => a.Following)
                .HasForeignKey(f => f.Account1Id);
            
            modelBuilder.Entity<Follow>()
                .HasOne<Account>(f => f.Account2)
                .WithMany(a => a.Followers)
                .HasForeignKey(f => f.Account2Id);
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Follow> Follows { get; set; }
    }

}