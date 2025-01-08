namespace E_Commerce_API.Modelos
{
    public class Rol
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public ICollection<Usuario> Usuario { get; set; }
    }
}
