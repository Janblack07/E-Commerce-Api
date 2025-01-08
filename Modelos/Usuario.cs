namespace E_Commerce_API.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Apellido { get; set; }
        public String Cedula { get; set; }
        public String Telefono { get; set; }

        public String Direccion { get; set; }

        public String Email { get; set; }
        public String Password { get; set; }

        public int RolId { get; set; }
        public Rol Rol { get; set; }

        public ICollection<Carrito> Carrito { get; set; }
        public ICollection<Pedido> Pedido { get; set; }

    }
}
