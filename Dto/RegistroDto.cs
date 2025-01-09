namespace E_Commerce_API.Dto
{
    public class RegistroDto
    {
        public String Nombres { get; set; }
        public String Apellidos { get; set; }
        public String Cedula { get; set; }
        public String Telefono { get; set; }

        public String Direccion { get; set; }

        public String Email { get; set; }
        public String Password { get; set; }

        public int RolId { get; set; }
    }
}
