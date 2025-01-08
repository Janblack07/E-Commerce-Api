namespace E_Commerce_API.Modelos
{
    public class MetodoPago
    {
        public int Id { get; set; }

        public String nombre { get; set; }
        public ICollection<Pedido> Pedido { get; set; }
    }
}
