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
    public class MetodoPagoController : ControllerBase
    {
        private readonly DbEcommerce _context;
        public MetodoPagoController(DbEcommerce context)
        {
            _context = context;
        }
        [HttpPost("Crear")]
        public async Task<IActionResult> CrearPedido(CrearPedidoDto crearPedidoDto)
        {
            if (crearPedidoDto == null || crearPedidoDto.crearDetallePedido == null || !crearPedidoDto.crearDetallePedido.Any())
            {
                return BadRequest(new { message = "El pedido debe contener al menos un detalle de pedido." });
            }

            // Validar si el método de pago existe
            var metodoPago = await _context.MetodosPago.FindAsync(crearPedidoDto.MetodoPagoId);
            if (metodoPago == null)
            {
                return BadRequest(new { message = "El método de pago no existe." });
            }

            float total = 0;
            var detallesPedido = new List<DetallePedido>();

            foreach (var detalleDto in crearPedidoDto.crearDetallePedido)
            {
                var producto = await _context.Productos.FindAsync(detalleDto.ProductoId);
                if (producto == null)
                {
                    return BadRequest(new { message = $"El producto con ID {detalleDto.ProductoId} no existe." });
                }

                // Calcular subtotal para el detalle
                total += producto.Precio * detalleDto.Cantidad;

                // Crear detalle de pedido
                detallesPedido.Add(new DetallePedido
                {
                    Cantidad = detalleDto.Cantidad,
                    Precio = producto.Precio,
                    ProductoId = producto.Id
                });
            }

            // Crear el pedido
            var pedido = new Pedido
            {
                fecha = DateOnly.FromDateTime(DateTime.Now),
                Total = total,
                UsuarioId = 1, // Reemplaza con el ID del usuario autenticado
                MetodoPagoId = crearPedidoDto.MetodoPagoId,
                DetallePedido = detallesPedido
            };

            // Guardar en la base de datos
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido creado exitosamente.", pedidoId = pedido.Id });
        }

        // Obtener Método de Pago por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerMetodoPago(int id)
        {
            var metodoPago = await _context.MetodosPago.FindAsync(id);

            if (metodoPago == null)
                return NotFound(new { message = "Método de pago no encontrado." });

            var metodoPagoDto = new MetodoPagoDto
            {
                Id = metodoPago.Id,
                Nombre = metodoPago.nombre
            };

            return Ok(new { message = "Método de pago obtenido exitosamente.", metodoPago = metodoPagoDto });
        }

        // Listar Métodos de Pago
        [HttpGet]
        public async Task<IActionResult> ListarMetodosPago()
        {
            var metodosPago = await _context.MetodosPago
                .Select(mp => new MetodoPagoDto
                {
                    Id = mp.Id,
                    Nombre = mp.nombre
                }).ToListAsync();

            return Ok(new { message = "Métodos de pago obtenidos exitosamente.", metodosPago });
        }
    }
}
