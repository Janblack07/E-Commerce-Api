namespace E_Commerce_API.Modelos
{
    public class DetallePedido
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public float Precio { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }
    }
}
