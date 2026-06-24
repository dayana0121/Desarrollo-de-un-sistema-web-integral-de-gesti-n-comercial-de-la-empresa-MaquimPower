using Microsoft.EntityFrameworkCore;
using E1.Models;

namespace E1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Configuración
        public DbSet<Empresa> Empresas { get; set; }

        // Seguridad
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<AccesoPorRol> AccesosPorRol { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        // Terceros
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }

        // Productos
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Producto> Productos { get; set; }

        // Compras
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetalleCompras { get; set; }

        // Inventario
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }
        public DbSet<Traslado> Traslados { get; set; }
        public DbSet<DetalleTraslado> DetalleTraslados { get; set; }

        // Caja
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }

        // Ventas
        public DbSet<EstadoVenta> EstadosVenta { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetalleCotizaciones { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
    }
}