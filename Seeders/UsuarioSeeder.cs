using E_Commerce_API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_API.Seeders
{
    public class UsuarioSeeder
    {
        public static void SeedAdminUser(ModelBuilder modelBuilder)
        {
            var password = BCrypt.Net.BCrypt.HashPassword("Admin123");

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Nombres = "Admin",
                    Apellidos= "Admin",
                    Cedula="1234567890",
                    Email = "admin@tienda.com",
                    Password = password,
                    Direccion = "123 Admin St",
                    Telefono = "098912345",
                    RolId = 1 // Rol Administrador
                }
            );
        }
    }
}
