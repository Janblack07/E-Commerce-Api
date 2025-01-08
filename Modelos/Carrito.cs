namespace E_Commerce_API.Modelos
{
    public class Carrito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public ICollection<DetalleCarrito> DetalleCarrito { get; set; }
    }
}
