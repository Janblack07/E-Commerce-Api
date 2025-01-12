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
    public class VentaController : ControllerBase
    {
        private readonly DbEcommerce _context;

        public VentaController(DbEcommerce context)
        {
            _context = context;
        }
        // Obtener ventas por rol
        [HttpGet]
      
        public async Task<IActionResult> ObtenerVentas()
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role != "Administrador")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
            }

            var ventas = await _context.Ventas
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.Producto)
                .Include(v => v.Pedido)
                .ToListAsync();

            return Ok(ventas);
        }


        // Crear una nueva venta
        [HttpPost]
        [Route("Crear")]
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaDto crearVentaDto)
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role == "Empleado")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Cliente." });
            }

            // Validar el pedido
            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedido)
                .ThenInclude(dp => dp.Producto)
                .FirstOrDefaultAsync(p => p.Id == crearVentaDto.PedidoId);

            if (pedido == null)
            {
                return BadRequest(new { message = "El pedido no existe." });
            }


            // Verificar si el pedido ya está vinculado a una venta
            var ventaExistente = await _context.Ventas.AnyAsync(v => v.PedidoId == pedido.Id);
            if (ventaExistente)
            {
                return BadRequest(new { message = "El pedido ya está asociado a una venta." });
            }

            // Crear una nueva venta
            var venta = new Venta
            {
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                PedidoId = pedido.Id,
                Total = 0,
                DetalleVenta = new List<DetalleVenta>()
            };

            // Calcular el total y los detalles de la venta
            foreach (var detallePedido in pedido.DetallePedido)
            {
                var detalleVenta = new DetalleVenta
                {
                    ProductoId = detallePedido.ProductoId,
                    Cantidad = detallePedido.Cantidad,
                    Precio = detallePedido.Producto.Precio, // Precio del producto
                    Venta = venta
                };

                venta.DetalleVenta.Add(detalleVenta);
                venta.Total += detalleVenta.Precio * detalleVenta.Cantidad; // Calcular subtotal
            }

            // Guardar la venta en la base de datos
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Venta creada exitosamente.", venta });
        }

        // Obtener venta por ID
        [HttpGet]
        [Route("{id}")]
       
        public async Task<IActionResult> ObtenerVentaPorId(int id)
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role == "Empleado")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Cliente." });
            }
            var venta = await _context.Ventas
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.Producto)
                .Include(v => v.Pedido)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound(new { message = "La venta no existe." });
            }

            return Ok(venta);
        }
        // Eliminar una venta
        [HttpDelete]
        [Route("Eliminar/{id}")]
       
        public async Task<IActionResult> EliminarVenta(int id)
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role != "Administrador")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
            }
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound(new { message = "La venta no existe." });
            }

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Venta eliminada exitosamente." });
        }

        // Obtener ventas diarias
        [HttpGet]
        [Route("Diarias")]
        public async Task<IActionResult> ObtenerVentasDiarias()
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role != "Administrador")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
            }
            // Obtener la fecha actual
            var fechaActual = DateOnly.FromDateTime(DateTime.Now);

            // Filtrar las ventas por la fecha actual
            var ventasDiarias = await _context.Ventas
                .Where(v => v.Fecha == fechaActual)
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.Producto)
                .ToListAsync();

            // Calcular el total de las ventas diarias
            var totalVentasDiarias = ventasDiarias.Sum(v => v.Total);

            return Ok(new
            {
                message = "Ventas diarias obtenidas exitosamente.",
                total = totalVentasDiarias,
                ventas = ventasDiarias
            });
        }

        // Obtener ventas mensuales
        [HttpGet]
        [Route("Mensuales")]
        public async Task<IActionResult> ObtenerVentasMensuales()
        {
            // Obtener el rol desde el token (ya está validado por [Authorize])
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            // Validar si el usuario no es administrador
            if (role != "Administrador")
            {
                return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
            }
            // Obtener el mes y año actuales
            var fechaActual = DateTime.Now;
            var mesActual = fechaActual.Month;
            var anioActual = fechaActual.Year;

            // Filtrar las ventas por el mes y año actuales
            var ventasMensuales = await _context.Ventas
                .Where(v => v.Fecha.Month == mesActual && v.Fecha.Year == anioActual)
                .Include(v => v.DetalleVenta)
                .ThenInclude(dv => dv.Producto)
                .ToListAsync();

            // Calcular el total de las ventas mensuales
            var totalVentasMensuales = ventasMensuales.Sum(v => v.Total);

            return Ok(new
            {
                message = "Ventas mensuales obtenidas exitosamente.",
                total = totalVentasMensuales,
                ventas = ventasMensuales
            });
        }


    }
}
