namespace E_Commerce_API.Modelos
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public float Precio { get; set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        public int VentaId { get; set; }
        public Venta Venta { get; set; }
    }
}
