using E_Commerce_API.Seeders;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_API.Servicios
{
    public class DbEcommerce : DbContext
    {
        public DbEcommerce(DbContextOptions<DbEcommerce> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Seeders//
            RolSeeder.SeedRoles(modelBuilder);
            UsuarioSeeder.SeedAdminUser(modelBuilder);

        }
    }
}
