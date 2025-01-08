using E_Commerce_API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_API.Seeders
{
    public class RolSeeder
    {
        public static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador" },
                new Rol { Id = 2, Nombre = "Empleado" },
                new Rol { Id = 3, Nombre = "Cliente" }
            );
        }
    }
}
