using Microsoft.EntityFrameworkCore;

namespace LinkedHU_CENG.Models
{
    public class Context : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Server=18.212.186.54;Port=5432;Database=linkedhu;User Id=admin;Password=qweasdzxc;");
        }
        public DbSet<Account> Accounts { get; set; }
    }

}