namespace E_Commerce_API.Modelos
{
    public class Producto
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public float Precio { get; set; }
        public String ImagenUrl { get; set; }


        public int CategoriaId { get; set; }

        public Categoria Categoria { get; set; }

        public ICollection<Inventario> Inventario { get; set; }
        public ICollection<DetalleCarrito> DetalleCarrito { get; set; }
    }
}
