using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E1.Data;
using E1.Models;

namespace E1.Controllers
{
    public class InventarioController : Controller
    {
        private readonly AppDbContext _context;

        public InventarioController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Movimientos()
        {
            return View();
        }

        public IActionResult Traslados()
        {
            return View("~/Views/Inventario/Traslados.cshtml");
        }

        // =========================
        // ALMACENES
        // =========================

        [HttpGet]
        public JsonResult ListarAlmacenes()
        {
            var almacenes = _context.Almacenes
                .OrderByDescending(a => a.id_almacen)
                .Select(a => new
                {
                    a.id_almacen,
                    a.codigo,
                    a.nombre,
                    a.ubicacion,
                    a.estado
                })
                .ToList();

            return Json(new { data = almacenes });
        }

        [HttpPost]
        public JsonResult GuardarAlmacen(Almacen almacen)
        {
            try
            {
                if (almacen.id_almacen == 0)
                {
                    almacen.fecha_creacion = DateTime.Now;
                    almacen.fecha_modificacion = DateTime.Now;
                    _context.Almacenes.Add(almacen);
                }
                else
                {
                    var almacenDb = _context.Almacenes.Find(almacen.id_almacen);

                    if (almacenDb == null)
                        return Json(new { success = false, mensaje = "Almacén no encontrado." });

                    almacenDb.codigo = almacen.codigo;
                    almacenDb.nombre = almacen.nombre;
                    almacenDb.ubicacion = almacen.ubicacion;
                    almacenDb.estado = almacen.estado;
                    almacenDb.fecha_modificacion = DateTime.Now;
                }

                _context.SaveChanges();

                return Json(new { success = true, mensaje = "Almacén guardado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EliminarAlmacen(int id_almacen)
        {
            try
            {
                var almacen = _context.Almacenes.Find(id_almacen);

                if (almacen == null)
                    return Json(new { success = false, mensaje = "Almacén no encontrado." });

                almacen.estado = false;
                almacen.fecha_modificacion = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, mensaje = "Almacén desactivado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        // =========================
        // MOVIMIENTOS / KARDEX
        // =========================

        [HttpGet]
        public JsonResult ListarMovimientos()
        {
            var movimientos = _context.MovimientosStock
                .Include(m => m.Producto)
                .Include(m => m.Almacen)
                .OrderByDescending(m => m.id_movimiento)
                .Select(m => new
                {
                    fecha = m.fecha_creacion.ToString("dd/MM/yyyy HH:mm"),
                    producto = m.Producto != null ? m.Producto.nombre : "",
                    almacen = m.Almacen != null ? m.Almacen.nombre : "",
                    m.tipo_movimiento,
                    m.tipo_referencia,
                    m.cantidad,
                    m.stock_anterior,
                    m.stock_resultante,
                    m.observacion
                })
                .ToList();

            return Json(new { data = movimientos });
        }

        // =========================
        // COMBOS
        // =========================

        [HttpGet]
        public JsonResult ListarAlmacenesActivos()
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
        public JsonResult ListarProductosActivos()
        {
            var productos = _context.Productos
                .Where(p => p.estado)
                .Select(p => new
                {
                    p.id_producto,
                    p.nombre,
                    p.stock_actual
                })
                .ToList();

            return Json(productos);
        }

        // =========================
        // TRASLADOS
        // =========================

        [HttpGet]
        public JsonResult ListarTraslados()
        {
            var traslados = _context.Traslados
                .Include(t => t.AlmacenOrigen)
                .Include(t => t.AlmacenDestino)
                .OrderByDescending(t => t.id_traslado)
                .Select(t => new
                {
                    t.id_traslado,
                    t.numero_traslado,
                    origen = t.AlmacenOrigen != null ? t.AlmacenOrigen.nombre : "",
                    destino = t.AlmacenDestino != null ? t.AlmacenDestino.nombre : "",
                    fecha = t.fecha_traslado.ToString("dd/MM/yyyy"),
                    t.estado,
                    t.observacion
                })
                .ToList();

            return Json(new { data = traslados });
        }

        [HttpPost]
        public JsonResult GuardarTraslado([FromBody] TrasladoRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                if (request == null || request.detalles == null || !request.detalles.Any())
                    return Json(new { success = false, mensaje = "Debe agregar al menos un producto." });

                if (request.id_almacen_origen == request.id_almacen_destino)
                    return Json(new { success = false, mensaje = "El almacén origen y destino no pueden ser iguales." });

                var traslado = new Traslado
                {
                    numero_traslado = GenerarNumeroTraslado(),
                    id_almacen_origen = request.id_almacen_origen,
                    id_almacen_destino = request.id_almacen_destino,
                    id_usuario = request.id_usuario,
                    fecha_traslado = DateTime.Now,
                    estado = "Pendiente",
                    observacion = request.observacion,
                    fecha_creacion = DateTime.Now,
                    fecha_modificacion = DateTime.Now
                };

                _context.Traslados.Add(traslado);
                _context.SaveChanges();

                foreach (var item in request.detalles)
                {
                    var detalle = new DetalleTraslado
                    {
                        id_traslado = traslado.id_traslado,
                        id_producto = item.id_producto,
                        cantidad = item.cantidad,
                        fecha_creacion = DateTime.Now,
                        fecha_modificacion = DateTime.Now
                    };

                    _context.DetalleTraslados.Add(detalle);
                }

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, mensaje = "Traslado registrado correctamente." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult DetalleTraslado(int id_traslado)
        {
            var detalle = _context.DetalleTraslados
                .Include(d => d.Producto)
                .Where(d => d.id_traslado == id_traslado)
                .Select(d => new
                {
                    producto = d.Producto != null ? d.Producto.nombre : "",
                    d.cantidad
                })
                .ToList();

            return Json(new { data = detalle });
        }

        [HttpPost]
        public JsonResult ConfirmarTraslado(int id_traslado)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var traslado = _context.Traslados
                    .FirstOrDefault(t => t.id_traslado == id_traslado);

                if (traslado == null)
                    return Json(new { success = false, mensaje = "Traslado no encontrado." });

                if (traslado.estado != "Pendiente")
                    return Json(new { success = false, mensaje = "Solo se puede confirmar un traslado pendiente." });

                var detalles = _context.DetalleTraslados
                    .Where(d => d.id_traslado == id_traslado)
                    .ToList();

                foreach (var detalle in detalles)
                {
                    var producto = _context.Productos
                        .FirstOrDefault(p => p.id_producto == detalle.id_producto);

                    if (producto == null)
                        continue;

                    if (producto.stock_actual < detalle.cantidad)
                    {
                        return Json(new
                        {
                            success = false,
                            mensaje = $"Stock insuficiente para el producto {producto.nombre}."
                        });
                    }

                    int stockAnterior = producto.stock_actual;
                    int stockSalida = stockAnterior - detalle.cantidad;
                    int stockEntrada = stockSalida + detalle.cantidad;

                    producto.stock_actual = stockSalida;
                    producto.fecha_modificacion = DateTime.Now;

                    _context.MovimientosStock.Add(new MovimientoStock
                    {
                        id_producto = producto.id_producto,
                        id_almacen = traslado.id_almacen_origen,
                        id_usuario = traslado.id_usuario,
                        tipo_movimiento = "SALIDA",
                        tipo_referencia = "TRASLADO",
                        id_referencia = traslado.id_traslado,
                        cantidad = detalle.cantidad,
                        stock_anterior = stockAnterior,
                        stock_resultante = stockSalida,
                        observacion = $"Salida por traslado {traslado.numero_traslado}",
                        fecha_creacion = DateTime.Now,
                        fecha_modificacion = DateTime.Now
                    });

                    _context.MovimientosStock.Add(new MovimientoStock
                    {
                        id_producto = producto.id_producto,
                        id_almacen = traslado.id_almacen_destino,
                        id_usuario = traslado.id_usuario,
                        tipo_movimiento = "ENTRADA",
                        tipo_referencia = "TRASLADO",
                        id_referencia = traslado.id_traslado,
                        cantidad = detalle.cantidad,
                        stock_anterior = stockSalida,
                        stock_resultante = stockEntrada,
                        observacion = $"Entrada por traslado {traslado.numero_traslado}",
                        fecha_creacion = DateTime.Now,
                        fecha_modificacion = DateTime.Now
                    });
                }

                traslado.estado = "Confirmado";
                traslado.fecha_modificacion = DateTime.Now;

                _context.SaveChanges();
                transaction.Commit();

                return Json(new { success = true, mensaje = "Traslado confirmado correctamente." });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AnularTraslado(int id_traslado)
        {
            try
            {
                var traslado = _context.Traslados.Find(id_traslado);

                if (traslado == null)
                    return Json(new { success = false, mensaje = "Traslado no encontrado." });

                if (traslado.estado != "Pendiente")
                    return Json(new { success = false, mensaje = "Solo se puede anular un traslado pendiente." });

                traslado.estado = "Anulado";
                traslado.fecha_modificacion = DateTime.Now;

                _context.SaveChanges();

                return Json(new { success = true, mensaje = "Traslado anulado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        private string GenerarNumeroTraslado()
        {
            int total = _context.Traslados.Count() + 1;
            return $"TR-{total.ToString("D6")}";
        }
    }

    public class TrasladoRequest
    {
        public int id_almacen_origen { get; set; }
        public int id_almacen_destino { get; set; }
        public int id_usuario { get; set; } = 1;
        public string? observacion { get; set; }
        public List<DetalleTrasladoRequest> detalles { get; set; } = new();
    }

    public class DetalleTrasladoRequest
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
    }
}