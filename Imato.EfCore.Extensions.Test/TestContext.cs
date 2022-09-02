using Microsoft.EntityFrameworkCore;

namespace Imato.EfCore.Extensions.Test
{
    public class TestContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Imato_EfCore_Extensions_Test;Trusted_Connection=True;");
        }
    }
}