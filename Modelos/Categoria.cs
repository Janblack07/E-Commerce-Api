namespace E_Commerce_API.Modelos
{
    public class Categoria
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public String ImagenUrl { get; set; }

        public ICollection<Producto> Producto { get; set; }
    }
}
