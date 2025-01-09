using E_Commerce_API.Modelos;

namespace E_Commerce_API.Dto
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public float Precio { get; set; }
        public String ImagenUrl { get; set; }
        public CategoriaDto Categoria { get; set; }
    }
    public class CreateProductoDto
    {
        public string Nombre { get; set; }
        public double Precio { get; set; }
        public string Descripcion { get; set; }
        public IFormFile Imagen { get; set; } 
        public int CategoriaId { get; set; } 
    }
    public class UpdateProductoDto
    {
        public string Nombre { get; set; }
        public double? Precio { get; set; } 
        public string Descripcion { get; set; }
        public IFormFile Imagen { get; set; } 
        public int? CategoriaId { get; set; } 
    }
}