namespace E_Commerce_API.Modelos
{
    public class Venta
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public float Total { get; set; }
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public ICollection<DetalleVenta> DetalleVenta { get; set; }
    }
}
