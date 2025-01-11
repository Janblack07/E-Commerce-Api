using E_Commerce_API.Dto;
using E_Commerce_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_API.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly Authentication _authService;
        private readonly DbEcommerce _context;
        public UsuarioController(Authentication authService, DbEcommerce context)
        {
            _authService = authService;
            _context = context;
        }
      
        [HttpPost]
        [Route("RegistroCliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] RegistroDto dto)
        {
            try
            {
                dto.RolId = 3; // Asignar rol fijo de cliente
                var usuario = await _authService.Register(dto);
                return Ok(new { message = "Cliente registrado exitosamente.", usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Registro de EMPLEADOS (solo autorizado por Administrador)
        [HttpPost]
        [Route("RegistroEmpleado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RegisterEmpleado([FromBody] RegistroDto dto)
        {
            try
            {
                if (dto.RolId != 2)
                    return BadRequest(new { message = "El rol debe ser 'Empleado' para este registro." });

                var usuario = await _authService.Register(dto);
                return Ok(new { message = "Empleado registrado exitosamente.", usuario });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

                // Verificar si el usuario no está registrado
                if (usuario == null)
                {
                    return Unauthorized(new { message = "El usuario no está registrado. Por favor, valla a  regístrarse." });
                }

           

                // Generar el token
                var token = await _authService.Login(dto);

                return Ok(new { message = "Usted ha iniciado sesión con éxito.", token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al procesar su solicitud.", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout()
        {
            try
            {

                // Invalida el token en el cliente (no se envía ningún token nuevo desde el servidor)
                return Ok(new { message = "Ha cerrado sesión con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al intentar cerrar sesión.", details = ex.Message });
            }
        }

        [HttpGet]
        [Route("Perfil")]
        [Authorize] // Solo usuarios autenticados
        public async Task<IActionResult> GetPerfil()
        {
            var userId = int.Parse(User.Identity.Name); // Extraer el ID desde el token
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            var perfilDto = new UsuarioDto
            {
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Cedula = usuario.Cedula,
                Email = usuario.Email,
                Direccion = usuario.Direccion,
                Telefono = usuario.Telefono,
                Rol = new RolDto
                {
                    Id = usuario.Rol.Id,
                    Nombre = usuario.Rol.Nombre
                }
            };

            return Ok(perfilDto);
        }

        [HttpGet]
        [Route("ListaEmpleados")]
        public async Task<IActionResult> GetEmpleados()
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role != "Administrador")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
            }

            var empleados = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol.Nombre == "Empleado")
                .ToListAsync();

            if (empleados == null || !empleados.Any())
            {
                return NotFound(new { message = "No se encontraron empleados registrados." });
            }

            var result = empleados.Select(e => new UsuarioDto
            {
                Nombres = e.Nombres,
                Apellidos = e.Apellidos,
                Cedula = e.Cedula,
                Email = e.Email,
                Direccion = e.Direccion,
                Telefono = e.Telefono,
                Rol = new RolDto
                {
                    Id = e.Rol.Id,
                    Nombre = e.Rol.Nombre
                }
            });

            return Ok(new { message = "Todos los empleados:", result });
        }


        // Listar clientes (solo para Administradores y Empleados)
        [HttpGet]
        [Route("ListaClientes")]
    
        public async Task<IActionResult> GetClientes()
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            
            // Validar si el usuario no es Administrador o Empleado
            if (role == "Cliente")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Empleados." });
            }
            var clientes = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol.Nombre == "Cliente")
                .ToListAsync();
           
            if (clientes == null || !clientes.Any())
            {
                return NotFound(new { message = "No se encontraron clientes registrados." });
            }
            var result = clientes.Select(c => new UsuarioDto
            {
              
                Nombres = c.Nombres,
                Apellidos=c.Apellidos,
                Cedula=c.Cedula,
                Email = c.Email,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Rol = new RolDto {
                    Id = c.Rol.Id,
                    Nombre = c.Rol.Nombre
                }
            });
           

            return Ok(new {message ="Todos los clientes : ", result });
        }
    }
}
