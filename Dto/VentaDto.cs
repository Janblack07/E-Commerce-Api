namespace E_Commerce_API.Dto
{
    public class VentaDto
    {
        public int Id { get; set; }
        public DateOnly fecha { get; set; }
        public float Total { get; set; }
        public int PedidoId { get; set; }
        public PedidoDto Pedido { get; set; }
        public ICollection<DetalleVentaDto> DetalleVenta { get; set; }
    }

    public class CrearVentaDto
    {
        public int PedidoId { get; set; } // Se envía el ID del pedido asociado
        public ICollection<CrearDetalleVentaDto> CrearDetalleVenta { get; set; }
    }

    public class DetalleVentaDto
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public float Precio { get; set; }
        public ProductoDto Producto { get; set; }
    }

    public class CrearDetalleVentaDto
    {
        public int ProductoId { get; set; } // Solo se necesita el ID del producto
        public int Cantidad { get; set; } // La cantidad se toma del carrito
    }
}
