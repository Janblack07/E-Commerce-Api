using API_TIENDA.Servicios;
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
    public class CarritoController : ControllerBase
    {
        private readonly DbEcommerce _context;
        private readonly ICloudinaryService _cloudinaryService;
        public CarritoController(DbEcommerce context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }
        // Agregar producto al carrito
        [HttpPost]
        [Route("AgregarProducto")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> AgregarProducto([FromBody] AgregarAlCarritoDto dto)
        {
            // Obtener el rol y el ID del usuario desde el token
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            // Obtener el ID del usuario desde el token
            var usuarioId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            // Verificar si el rol no es "Cliente"
            if (role != "Cliente")
            {
                return StatusCode(403, new { message = "Acceso denegado. Solo los clientes pueden agregar productos al carrito." });
            }

            // Obtener o crear el carrito para el usuario
            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarrito)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    UsuarioId = usuarioId,
                    DetalleCarrito = new List<DetalleCarrito>()
                };
                _context.Carritos.Add(carrito);
            }

            // Verificar si el producto ya está en el carrito
            var detalle = carrito.DetalleCarrito.FirstOrDefault(d => d.ProductoId == dto.ProductoId);

            if (detalle == null)
            {
                // Obtener información del producto
                var producto = await _context.Productos.FindAsync(dto.ProductoId);
                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado." });
                }

                // Agregar nuevo producto al carrito
                detalle = new DetalleCarrito
                {
                    ProductoId = dto.ProductoId,
                    Cantidad = dto.Cantidad,
                    Carrito = carrito
                };
                carrito.DetalleCarrito.Add(detalle);
            }
            else
            {
                // Actualizar cantidad si ya existe
                detalle.Cantidad += dto.Cantidad;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Producto agregado al carrito exitosamente." });
        }

        [HttpGet]
        [Route("ObtenerCarrito")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ObtenerCarrito()
        {
            // Obtener el rol y el ID del usuario desde el token
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var usuarioId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.Name).Value);

            // Verificar si el rol no es "Cliente"
            if (role != "Cliente")
            {
                return StatusCode(403, new { message = "Acceso denegado. Solo los clientes pueden obtener productos del carrito." });
            }

            // Obtener el carrito del usuario junto con los detalles y productos, incluyendo la categoría del producto
            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarrito)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria) // Incluir la categoría del producto
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            // Verificar si el carrito existe y si tiene detalles
            if (carrito == null || carrito.DetalleCarrito == null || !carrito.DetalleCarrito.Any())
            {
                return NotFound(new { message = "El carrito está vacío o no existe." });
            }

            // Mapear a DTOs de forma segura
            var carritoDto = new CarritoDto
            {
                Id = carrito.Id,
                DetalleCarrito = carrito.DetalleCarrito.Select(d => new DetalleCarritoDto
                {
                    Cantidad = d.Cantidad,
                    Producto = new ProductoDto
                    {
                        Id = d.Producto?.Id ?? 0, // Verificación nula
                        Nombre = d.Producto?.Nombre ?? "Producto desconocido", // Verificación nula
                        Descripcion = d.Producto?.Descripcion ?? "Descripción no disponible", // Verificación nula
                        Precio = d.Producto?.Precio ?? 0, // Verificación nula
                        ImagenUrl = d.Producto?.ImagenUrl ?? "url_no_disponible", // Verificación nula
                        Categoria = new CategoriaDto
                        {
                            Id = d.Producto?.Categoria?.Id ?? 0, // Verificación nula de la categoría
                            Nombre = d.Producto?.Categoria?.Nombre ?? "Categoría no disponible" // Verificación nula de la categoría
                        }
                    }
                }).ToList()
            };

            return Ok(new { message = "Carrito obtenido exitosamente.", carrito = carritoDto });
        }



        // Eliminar producto del carrito
        [HttpDelete]
        [Route("EliminarProducto/{productoId}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> EliminarProducto(int productoId)
        {
            // Obtener el rol y el ID del usuario desde el token
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var usuarioId = int.Parse(User.Claims.First(c => c.Type == ClaimTypes.Name).Value);

            // Verificar si el rol no es "Cliente"
            if (role != "Cliente")
            {
                return StatusCode(403, new { message = "Acceso denegado. Solo los clientes pueden eliminar productos del carrito." });
            }

            // Obtener el carrito del usuario
            var carrito = await _context.Carritos
                .Include(c => c.DetalleCarrito)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (carrito == null || carrito.DetalleCarrito == null || !carrito.DetalleCarrito.Any())
            {
                return NotFound(new { message = "El carrito está vacío." });
            }

            // Buscar el detalle del producto a eliminar en el carrito
            var detalle = carrito.DetalleCarrito.FirstOrDefault(d => d.ProductoId == productoId);
            if (detalle == null)
            {
                return NotFound(new { message = "El producto no está en el carrito." });
            }

            // Eliminar el detalle del carrito
            _context.DetallesCarrito.Remove(detalle);

            // Guardar los cambios
            await _context.SaveChangesAsync();

            return Ok(new { message = "Producto eliminado del carrito exitosamente." });
        }
    }
}
