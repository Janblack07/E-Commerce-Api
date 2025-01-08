using E_Commerce_API.Modelos;
using E_Commerce_API.Seeders;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_API.Servicios
{
    public class DbEcommerce : DbContext
    {
        public DbEcommerce(DbContextOptions<DbEcommerce> options) : base(options) { }

        // DbSets
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<DetalleCarrito> DetallesCarrito { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            // Carrito
            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Carrito)
                .HasForeignKey(c => c.UsuarioId);

            // DetalleCarrito
            modelBuilder.Entity<DetalleCarrito>()
                .HasOne(dc => dc.Carrito)
                .WithMany(c => c.DetalleCarrito)
                .HasForeignKey(dc => dc.CarritoId);

            modelBuilder.Entity<DetalleCarrito>()
                .HasOne(dc => dc.Producto)
                .WithMany(p => p.DetalleCarrito)
                .HasForeignKey(dc => dc.ProductoId);

            // Pedido
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedido)
                .HasForeignKey(p => p.UsuarioId);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.MetodoPago)
                .WithMany(mp => mp.Pedido)
                .HasForeignKey(p => p.MetodoPagoId);

            // DetallePedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Producto)
                .WithMany(p => p.DetallePedido)
                .HasForeignKey(dp => dp.ProductoId);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Pedido)
                .WithMany(p => p.DetallePedido)
                .HasForeignKey(dp => dp.PedidoId);

            // Venta
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Pedido)
                .WithMany(p => p.Venta)
                .HasForeignKey(v => v.PedidoId);

            // DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Producto)
                .WithMany(p => p.DetalleVenta)
                .HasForeignKey(dv => dv.ProductoId);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Venta)
                .WithMany(v => v.DetalleVenta)
                .HasForeignKey(dv => dv.VentaId);

            // Inventario
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithMany(p => p.Inventario)
                .HasForeignKey(i => i.ProductoId);

            // Producto
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Producto)
                .HasForeignKey(p => p.CategoriaId);

            // Usuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuario)
                .HasForeignKey(u => u.RolId);
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique(); // Garantiza que el email sea único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Cedula)
                .IsUnique(); // Garantiza que el cedula sea único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Telefono)
                .IsUnique(); // Garantiza que el telefono sea único

            //Seeders//
            RolSeeder.SeedRoles(modelBuilder);
            UsuarioSeeder.SeedAdminUser(modelBuilder);
            base.OnModelCreating(modelBuilder);

        }
    }
}
