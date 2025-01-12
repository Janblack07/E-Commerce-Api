namespace E_Commerce_API.Dto
{
    public class InventarioDto
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public int ProductoId { get; set; }
        public ProductoDto Producto { get; set; }
    }
    public class CrearActualizarInventarioDto
    {
        public int Cantidad { get; set; }
        public int ProductoId { get; set; }
    }
}
