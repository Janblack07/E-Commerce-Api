namespace E_Commerce_API.Modelos
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateOnly fecha { get; set; }
        public float Total { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int MetodoPagoId { get; set; }
        public MetodoPago MetodoPago { get; set; }

        public ICollection<DetallePedido> DetallePedido { get; set; }
        public ICollection<Venta> Venta { get; set; }
    }
}
