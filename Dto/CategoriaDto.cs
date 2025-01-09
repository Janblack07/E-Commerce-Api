namespace E_Commerce_API.Dto
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Descripcion { get; set; }
        public String ImagenUrl { get; set; }
    }
    public class CreateCategoriaDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public IFormFile Imagen { get; set; } 
    }
    public class UpdateCategoriaDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public IFormFile Imagen { get; set; } 
    }
}
