namespace E1.Models
{
    public class Dashboard
    {
        public decimal VentasHoy { get; set; }
        public int TotalProductos { get; set; }
        public int TotalClientes { get; set; }
        public int StockCritico { get; set; }
        public List<VentaMensual> VentasMensuales { get; set; } = new();
        public List<ProductoTop> TopProductos { get; set; } = new();
    }

    public class VentaMensual
    {
        public string mes { get; set; } = "";
        public decimal total_ventas { get; set; }
        public int cantidad_ventas { get; set; }
    }

    public class ProductoTop
    {
        public string nombre_producto { get; set; } = "";
        public int total_vendido { get; set; }
    }
}
