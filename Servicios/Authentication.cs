using E_Commerce_API.Dto;
using E_Commerce_API.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_Commerce_API.Servicios
{
    public class Authentication
    {
        private readonly DbEcommerce _context;
        private readonly IConfiguration _config;
        public Authentication(DbEcommerce context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        // Registro
        public async Task<Usuario> Register(RegistroDto dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == dto.RolId);
            if (role == null) throw new Exception("Rol no válido.");

            var usuario = new Usuario
            {
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Cedula = dto.Cedula,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                RolId = role.Id
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
        // Login
        public async Task<string> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password))
                throw new Exception("Credenciales incorrectas.");

            // Generar el token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre)
            }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
