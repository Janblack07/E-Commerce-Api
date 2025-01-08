namespace E_Commerce_API.Modelos
{
    public class Inventario
    {
        public int Id {  get; set; }

        public int Cantidad { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
