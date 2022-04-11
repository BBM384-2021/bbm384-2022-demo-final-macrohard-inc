using Microsoft.EntityFrameworkCore;
using LinkedHU_CENG.Models;
namespace LinkedHU_CENG.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<Account> Accounts { get; set; }
    }

}