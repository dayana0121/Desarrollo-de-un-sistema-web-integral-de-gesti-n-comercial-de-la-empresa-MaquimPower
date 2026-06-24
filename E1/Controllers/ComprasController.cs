using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E1.Data;
using E1.Models;

namespace E1.Controllers
{
    public class ComprasController : Controller
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult ListarCompras()
        {
            var compras = _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Almacen)
                .OrderByDescending(c => c.id_compra)
                .Select(c => new
                {
                    c.id_compra,
                    c.numero_compra,
                    proveedor = c.Proveedor != null ? c.Proveedor.razon_social : "",
                    almacen = c.Almacen != null ? c.Almacen.nombre : "",
                    fecha_compra = c.fecha_compra.ToString("dd/MM/yyyy"),
                    c.subtotal,
                    c.igv,
                    c.total,
                    c.estado,
                    c.observacion
                })
                .ToList();

            return Json(new { data = compras });
        }

        [HttpGet]
        public JsonResult ListarProveedores()
        {
            var proveedores = _context.Proveedores
                .Where(p => p.estado)
                .Select(p => new
                {
                    p.id_proveedor,
                    p.razon_social
                })
                .ToList();

            return Json(proveedores);
        }

        [HttpGet]
        public JsonResult ListarAlmacenes()
        {
            var almacenes = _context.Almacenes
                .Where(a => a.estado)
                .Select(a => new
                {
                    a.id_almacen,
                    a.nombre
                })
                .ToList();

            return Json(almacenes);
        }

        [HttpGet]
        public JsonResult ListarProductos()
        {
            var productos = _context.Productos
                .Where(p => p.estado)
                .Select(p => new
                {
                    p.id_producto,
                    p.nombre,
                    p.precio_costo,
                    p.stock_actual
                })
                .ToList();

            return Json(productos);
        }

        [HttpPost]
        public JsonResult GuardarCompra([FromBody] CompraRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                if (request == null || request.detalles == null || !request.detalles.Any())
                {
                    return Json(new { success = false, mensaje = "Debe agregar al menos un producto." });
                }

                decimal subtotal = request.detalles.Sum(d => d.cantidad * d.precio_unitario);
                decimal igv = subtotal * 0.18m;
                decimal total = subtotal + igv;

                var compra = new Compra
                {
                    id_proveedor = request.id_proveedor,
                    id_almacen = request.id_almacen,
                    id_usuario = request.id_usuario,
                    numero_compra = GenerarNumeroCompra(),
                    fecha_compra = DateTime.Now,
                    subtotal = subtotal,
                    igv = igv,
                    total = total,
                    estado = "Pendiente",
                    observacion = request.observacion,
                    fecha_creacion = DateTime.Now,
                    fecha_modificacion = DateTime.Now
                };

                _context.Compras.Add(compra);
                _context.SaveChanges();

                foreach (var item in request.detalles)
                {
                    var detalle = new DetalleCompra
                    {
                        id_compra = compra.id_compra,
                        id_producto = item.id_producto,
                        cantidad = item.cantidad,
                        precio_unitario = item.precio_unitario,
                        subtotal = item.cantidad * item.precio_unitario,
                        fecha_creacion = DateTime.Now,
                        fecha_modificacion = DateTime.Now
                    };

                    _context.DetalleCompras.Add(detalle);
                }

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, mensaje = "Compra registrada correctamente." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RecepcionarCompra(int id_compra)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var compra = _context.Compras.FirstOrDefault(c => c.id_compra == id_compra);

                if (compra == null)
                    return Json(new { success = false, mensaje = "Compra no encontrada." });

                if (compra.estado != "Pendiente")
                    return Json(new { success = false, mensaje = "Solo se puede recepcionar una compra pendiente." });

                var detalles = _context.DetalleCompras
                    .Where(d => d.id_compra == id_compra)
                    .ToList();

                foreach (var detalle in detalles)
                {
                    var producto = _context.Productos.FirstOrDefault(p => p.id_producto == detalle.id_producto);

                    if (producto == null)
                        continue;

                    int stockAnterior = producto.stock_actual;
                    int stockNuevo = stockAnterior + detalle.cantidad;

                    producto.stock_actual = stockNuevo;
                    producto.precio_costo = detalle.precio_unitario;
                    producto.fecha_modificacion = DateTime.Now;

                    var movimiento = new MovimientoStock
                    {
                        id_producto = producto.id_producto,
                        id_almacen = compra.id_almacen,
                        id_usuario = compra.id_usuario,
                        tipo_movimiento = "ENTRADA",
                        tipo_referencia = "COMPRA",
                        id_referencia = compra.id_compra,
                        cantidad = detalle.cantidad,
                        stock_anterior = stockAnterior,
                        stock_resultante = stockNuevo,
                        observacion = $"Recepción de compra {compra.numero_compra}",
                        fecha_creacion = DateTime.Now,
                        fecha_modificacion = DateTime.Now
                    };

                    _context.MovimientosStock.Add(movimiento);
                }

                compra.estado = "Recepcionado";
                compra.fecha_modificacion = DateTime.Now;

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, mensaje = "Compra recepcionada. El stock fue actualizado." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AnularCompra(int id_compra)
        {
            try
            {
                var compra = _context.Compras.FirstOrDefault(c => c.id_compra == id_compra);

                if (compra == null)
                    return Json(new { success = false, mensaje = "Compra no encontrada." });

                if (compra.estado != "Pendiente")
                    return Json(new { success = false, mensaje = "Solo se puede anular una compra pendiente." });

                compra.estado = "Anulado";
                compra.fecha_modificacion = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, mensaje = "Compra anulada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        private string GenerarNumeroCompra()
        {
            int total = _context.Compras.Count() + 1;
            return $"OC-{total.ToString("D6")}";
        }
    }

    public class CompraRequest
    {
        public int id_proveedor { get; set; }
        public int id_almacen { get; set; }
        public int id_usuario { get; set; } = 1;
        public string? observacion { get; set; }
        public List<DetalleCompraRequest> detalles { get; set; } = new();
    }

    public class DetalleCompraRequest
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio_unitario { get; set; }
    }
}