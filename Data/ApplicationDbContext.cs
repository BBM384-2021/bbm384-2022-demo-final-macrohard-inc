using LinkedHUCENGv2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LinkedHUCENGv2.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
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
        base.OnModelCreating(modelBuilder);

    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Follow> Follows { get; set; }

    public DbSet<Post> Post { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<PDF> Pdfs { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
}