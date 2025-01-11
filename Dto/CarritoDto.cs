namespace E_Commerce_API.Dto
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public DetalleCarritoDto DetalleCarrito { get; set; }

    }
    public class DetalleCarritoDto
    {
        
       
        public float Precio { get; set; }
        public int Cantidad { get; set; }
        public float Total => Precio * Cantidad;

        public ProductoDto Producto { get; set; }
    }
    public class AgregarAlCarritoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
