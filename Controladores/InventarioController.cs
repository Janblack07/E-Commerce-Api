using E_Commerce_API.Dto;
using E_Commerce_API.Modelos;
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
    [Authorize]
    public class InventarioController : ControllerBase
    {
        private readonly DbEcommerce _context;

        public InventarioController(DbEcommerce context)
        {
            _context = context;
        }
        // Obtener todos los inventarios
        [HttpGet]
        public async Task<IActionResult> ObtenerInventarios()
        {
            var inventarios = await _context.Inventarios
                .Include(i => i.Producto)
                .Select(i => new InventarioDto
                {
                    Id = i.Id,
                    Cantidad = i.Cantidad,
                    ProductoId = i.ProductoId,
                    Producto =   new ProductoDto
                    {
                        Id = i.Producto.Id,
                        Nombre = i.Producto.Nombre,
                        Precio = i.Producto.Precio,
                        Descripcion = i.Producto.Descripcion,
                        ImagenUrl = i.Producto.ImagenUrl,
                        Categoria = new CategoriaDto
                        {
                            Id = i.Producto.Categoria.Id,
                            Nombre = i.Producto.Categoria.Nombre,
                            Descripcion = i.Producto.Categoria.Descripcion,
                            ImagenUrl = i.Producto.Categoria.ImagenUrl,
                        }
                    }
                })
                .ToListAsync();

            return Ok(new { message = "Inventarios obtenidos exitosamente.", inventarios });
        }

        // Crear inventario
        [HttpPost]
        [Route ("Crear")]
        public async Task<IActionResult> CrearInventario([FromBody] CrearActualizarInventarioDto crearInventarioDto)
        {
          
                // Obtener el rol desde el token (ya está validado por [Authorize])
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Validar si el usuario no es administrador
                if (role == "Cliente")
                {
                    return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Empleados." });
                }
                var producto = await _context.Productos.FindAsync(crearInventarioDto.ProductoId);
            if (producto == null)
            {
                return BadRequest(new { message = "El producto no existe." });
            }

            var inventario = new Inventario
            {
                Cantidad = crearInventarioDto.Cantidad,
                ProductoId = crearInventarioDto.ProductoId
            };

            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Inventario creado exitosamente.", inventario });
        }

        // Actualizar inventario
        [HttpPut]
        [Route("Editar/{id}")]
        
        public async Task<IActionResult> ActualizarInventario(int id, [FromBody] CrearActualizarInventarioDto actualizarInventarioDto)
        {

            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role == "Cliente")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Empleados." });
            }
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
            {
                return NotFound(new { message = "El inventario no existe." });
            }

            var producto = await _context.Productos.FindAsync(actualizarInventarioDto.ProductoId);
            if (producto == null)
            {
                return BadRequest(new { message = "El producto no existe." });
            }

            inventario.Cantidad = actualizarInventarioDto.Cantidad;
            inventario.ProductoId = actualizarInventarioDto.ProductoId;

            _context.Inventarios.Update(inventario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Inventario actualizado exitosamente.", inventario });
        }

        // Eliminar inventario
        [HttpDelete]
        [Route("Eliminar/{id}")]
        public async Task<IActionResult> EliminarInventario(int id)
        {

            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role != "Administrador")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
            }
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
            {
                return NotFound(new { message = "El inventario no existe." });
            }

            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Inventario eliminado exitosamente." });
        }

        // Consultar inventario por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerInventarioPorId(int id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.Id == id)
                .Select(i => new InventarioDto
                {
                    Id = i.Id,
                    Cantidad = i.Cantidad,
                    ProductoId = i.ProductoId,
                    Producto = new ProductoDto
                    {
                        Id = i.Producto.Id,
                        Nombre = i.Producto.Nombre,
                        Precio = i.Producto.Precio,
                        Descripcion = i.Producto.Descripcion,
                        ImagenUrl = i.Producto.ImagenUrl,
                        Categoria = new CategoriaDto
                        {
                            Id = i.Producto.Categoria.Id,
                            Nombre = i.Producto.Categoria.Nombre,
                            Descripcion = i.Producto.Categoria.Descripcion,
                            ImagenUrl = i.Producto.Categoria.ImagenUrl,
                        }
                    }
                })
                .FirstOrDefaultAsync();

            if (inventario == null)
            {
                return NotFound(new { message = "El inventario no existe." });
            }

            return Ok(new { message = "Inventario obtenido exitosamente.", inventario });
        }
    }
}
