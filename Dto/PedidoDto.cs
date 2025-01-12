namespace E_Commerce_API.Dto
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public float Total { get; set; }
        public UsuarioDto Usuario { get; set; }
        public MetodoPagoDto MetodoPago { get; set; }
        public ICollection<DetallePedidoDto> DetallePedido { get; set; }
    }
    public class DetallePedidoDto
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public float Precio { get; set; }
        public ProductoDto Producto { get; set; }
    }
    public class CrearDetallePedidoDto
    {
        public int ProductoId { get; set; } // Solo necesitamos el ID del producto
        public int Cantidad { get; set; }
    }
    public class CrearPedidoDto
    {
        public int MetodoPagoId { get; set; } // Cambiar a int para solo enviar el ID
        public ICollection<CrearDetallePedidoDto> crearDetallePedido { get; set; }
    }

    public class MetodoPagoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
