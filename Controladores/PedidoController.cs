using E_Commerce_API.Dto;
using E_Commerce_API.Modelos;
using E_Commerce_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace E_Commerce_API.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly DbEcommerce _context;

        public PedidoController(DbEcommerce context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("{id}")]
      
        public async Task<IActionResult> ObtenerPedido(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedido)
                    .ThenInclude(d => d.Producto)
                .Include(p => p.MetodoPago)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound(new { message = "Pedido no encontrado." });

            var pedidoDto = new PedidoDto
            {
                Id = pedido.Id,
                Fecha = pedido.fecha,
                Total = pedido.Total,
                MetodoPago = new MetodoPagoDto
                {
                    Id = pedido.MetodoPago.Id,
                    Nombre = pedido.MetodoPago.nombre
                },
                DetallePedido = pedido.DetallePedido.Select(d => new DetallePedidoDto
                {
                    Id = d.Id,
                    Cantidad = d.Cantidad,
                    Precio = d.Precio,
                    Producto = new ProductoDto
                    {
                        Id = d.Producto.Id,
                        Nombre = d.Producto.Nombre,
                        Descripcion = d.Producto.Descripcion,
                        Precio = d.Producto.Precio,
                        ImagenUrl = d.Producto.ImagenUrl
                    }
                }).ToList()
            };

            return Ok(new { message = "Pedido obtenido exitosamente.", pedido = pedidoDto });
        }


        [HttpPost("Crear")]
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto crearPedidoDto)
        {
            // Validar si el DTO es nulo o no contiene detalles
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

            // Lista para almacenar los detalles del pedido
            var detallesPedido = new List<DetallePedido>();
            float total = 0;

            foreach (var detalleDto in crearPedidoDto.crearDetallePedido)
            {
                // Validar si el producto existe
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
                UsuarioId = 1, // Cambia esto según el ID del usuario autenticado
                MetodoPagoId = crearPedidoDto.MetodoPagoId,
                DetallePedido = detallesPedido
            };

            // Guardar en la base de datos
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido creado exitosamente.", pedidoId = pedido.Id });
        }

        // Eliminar Pedido
        [HttpDelete]
        [Route("Eliminar/{id}")]
        public async Task<IActionResult> EliminarPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
                return NotFound(new { message = "Pedido no encontrado." });

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pedido eliminado exitosamente." });
        }

    }
}
