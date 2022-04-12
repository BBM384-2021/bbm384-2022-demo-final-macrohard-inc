using Microsoft.EntityFrameworkCore;

namespace LinkedHU_CENG.Models
{
    public class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // For production (aws)
            optionsBuilder.UseNpgsql(
                "Server=18.212.186.54;Port=5432;Database=linkedhu;User Id=admin;Password=qweasdzxc;");
            // For development (local)
            // optionsBuilder.UseNpgsql(
            //    "Server=localhost;Port=5432;Database=linkedhu;User Id=admin;Password=qweasdzxc;");
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
        public DbSet<Follow> Follows { get; set; }
    }

}