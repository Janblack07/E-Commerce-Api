using E_Commerce_API.Modelos;

namespace E_Commerce_API.Dto
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public List<DetalleCarritoDto> DetalleCarrito { get; set; }

    }
    public class DetalleCarritoDto
    {
        
      
        public int Cantidad { get; set; }
        public ProductoDto Producto { get; set; }
    }
    public class AgregarAlCarritoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
   
}
