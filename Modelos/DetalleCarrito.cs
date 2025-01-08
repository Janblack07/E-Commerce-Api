namespace E_Commerce_API.Modelos
{
    public class DetalleCarrito
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
