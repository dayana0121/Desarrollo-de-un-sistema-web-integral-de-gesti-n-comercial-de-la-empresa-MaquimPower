using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using E1.Data;
using E1.Models;
using System.Text.Json;

namespace E1.Controllers
{
    public class VentasController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;

        public VentasController(AppDbContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }

        private int GetIdUsuario()
        {
            return _http.HttpContext!.Session.GetInt32("IdUsuario") ?? 0;
        }

        public IActionResult Index() => RedirectToAction("POS");

        // ── POS ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> POS()
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return RedirectToAction("Index", "Login");

            var cajaAbierta = await _db.Cajas
                .Where(c => c.id_usuario == idUsuario && c.estado == "Abierta")
                .FirstOrDefaultAsync();

            if (cajaAbierta == null)
            {
                TempData["Error"] = "Debes abrir una caja antes de realizar ventas.";
                return RedirectToAction("Index", "Caja");
            }

            ViewBag.IdCaja = cajaAbierta.id_caja;
            ViewBag.NombreCaja = cajaAbierta.nombre_caja;
            ViewBag.Clientes = await _db.Clientes
                .Where(c => c.estado == true)
                .OrderBy(c => c.nombre_completo)
                .ToListAsync();
            ViewBag.Almacenes = await _db.Almacenes
                .Where(a => a.estado == true)
                .ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string q)
        {
            var productos = await _db.Productos
                .Where(p => p.estado == true && p.stock_actual > 0 &&
                    (p.nombre.Contains(q) || p.sku.Contains(q)))
                .Select(p => new {
                    p.id_producto,
                    p.nombre,
                    p.sku,
                    p.precio_venta,
                    p.stock_actual
                })
                .Take(10)
                .ToListAsync();
            return Json(productos);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPOS([FromBody] RegistrarPOSRequest req)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return Json(new { ok = false, msg = "Sesión expirada" });
            try
            {
                string productosJson = JsonSerializer.Serialize(req.productos);
                using var conn = new SqlConnection(_db.Database.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_Ventas_RegistrarPOS", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_cliente", req.id_cliente);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@id_caja", req.id_caja);
                cmd.Parameters.AddWithValue("@id_almacen", req.id_almacen);
                cmd.Parameters.AddWithValue("@id_cotizacion", (object?)req.id_cotizacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tipo_comprobante", req.tipo_comprobante);
                cmd.Parameters.AddWithValue("@metodo_pago", req.metodo_pago);
                cmd.Parameters.AddWithValue("@tipo_documento_cli", (object?)req.tipo_documento_cli ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@numero_documento_cli", (object?)req.numero_documento_cli ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descuento", req.descuento);
                cmd.Parameters.AddWithValue("@monto_pagado", req.monto_pagado);
                cmd.Parameters.AddWithValue("@productos", productosJson);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int resultado = Convert.ToInt32(reader["resultado"]);
                    string mensaje = reader["mensaje"].ToString()!;
                    if (resultado > 0)
                    {
                        string numeroVenta = reader["numero_venta"].ToString()!;
                        decimal total = Convert.ToDecimal(reader["total"]);
                        decimal vuelto = Convert.ToDecimal(reader["vuelto"]);
                        return Json(new { ok = true, msg = mensaje, id_venta = resultado, numeroVenta, total, vuelto });
                    }
                    return Json(new { ok = false, msg = mensaje });
                }
                return Json(new { ok = false, msg = "Sin respuesta" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ── HISTORIAL ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Historial(string? busqueda, int? id_estado, string? fecha_desde, string? fecha_hasta)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return RedirectToAction("Index", "Login");

            var query = _db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoVenta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(v => v.numero_venta.Contains(busqueda) ||
                    v.Cliente!.nombre_completo.Contains(busqueda));

            if (id_estado.HasValue)
                query = query.Where(v => v.id_estado_venta == id_estado.Value);

            if (DateTime.TryParse(fecha_desde, out var desde))
                query = query.Where(v => v.fecha_venta >= desde);

            if (DateTime.TryParse(fecha_hasta, out var hasta))
                query = query.Where(v => v.fecha_venta <= hasta.AddDays(1));

            ViewBag.Ventas = await query.OrderByDescending(v => v.fecha_creacion).ToListAsync();
            ViewBag.Estados = await _db.EstadosVenta.Where(e => e.estado == true).ToListAsync();
            ViewBag.Busqueda = busqueda;
            ViewBag.IdEstado = id_estado;
            ViewBag.FechaDesde = fecha_desde;
            ViewBag.FechaHasta = fecha_hasta;
            return View();
        }

        // ── DETALLE ───────────────────────────────────────────────────────────
        public async Task<IActionResult> Detalle(int id)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return RedirectToAction("Index", "Login");

            var venta = await _db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoVenta)
                .FirstOrDefaultAsync(v => v.id_venta == id);

            if (venta == null) return NotFound();

            var detalle = await _db.DetalleVentas
                .Include(d => d.Producto)
                .Where(d => d.id_venta == id)
                .ToListAsync();

            ViewBag.Venta = venta;
            ViewBag.Detalle = detalle;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Anular(int id_venta)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return Json(new { ok = false, msg = "Sesión expirada" });
            try
            {
                using var conn = new SqlConnection(_db.Database.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_Ventas_Anular", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_venta", id_venta);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int resultado = Convert.ToInt32(reader["resultado"]);
                    string mensaje = reader["mensaje"].ToString()!;
                    return Json(new { ok = resultado > 0, msg = mensaje });
                }
                return Json(new { ok = false, msg = "Sin respuesta" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ── COTIZACIONES ──────────────────────────────────────────────────────
        public async Task<IActionResult> Cotizaciones(string? busqueda, string? estado)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return RedirectToAction("Index", "Login");

            var query = _db.Cotizaciones
                .Include(c => c.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(c => c.numero_cotizacion.Contains(busqueda) ||
                    c.Cliente!.nombre_completo.Contains(busqueda));

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.estado == estado);

            ViewBag.Cotizaciones = await query.OrderByDescending(c => c.fecha_creacion).ToListAsync();
            ViewBag.Clientes = await _db.Clientes.Where(c => c.estado == true)
                .OrderBy(c => c.nombre_completo).ToListAsync();
            ViewBag.Busqueda = busqueda;
            ViewBag.EstadoFiltro = estado;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] CrearCotizacionRequest req)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return Json(new { ok = false, msg = "Sesión expirada" });
            try
            {
                string productosJson = JsonSerializer.Serialize(req.productos);
                using var conn = new SqlConnection(_db.Database.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_Cotizaciones_Crear", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_cliente", req.id_cliente);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@observacion", (object?)req.observacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@productos", productosJson);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int resultado = Convert.ToInt32(reader["resultado"]);
                    string mensaje = reader["mensaje"].ToString()!;
                    string numero = resultado > 0 ? reader["numero_cotizacion"].ToString()! : "";
                    return Json(new { ok = resultado > 0, msg = mensaje, numero });
                }
                return Json(new { ok = false, msg = "Sin respuesta" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        // ── ESTADOS ───────────────────────────────────────────────────────────
        public async Task<IActionResult> Estados()
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return RedirectToAction("Index", "Login");
            ViewBag.Estados = await _db.EstadosVenta.OrderBy(e => e.nombre).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarEstado(int? id_estado_venta, string nombre, string codigo)
        {
            try
            {
                if (id_estado_venta.HasValue && id_estado_venta > 0)
                {
                    var est = await _db.EstadosVenta.FindAsync(id_estado_venta.Value);
                    if (est != null)
                    {
                        est.nombre = nombre;
                        est.codigo = codigo;
                        est.fecha_modificacion = DateTime.Now;
                    }
                }
                else
                {
                    _db.EstadosVenta.Add(new EstadoVenta
                    {
                        nombre = nombre,
                        codigo = codigo,
                        estado = true,
                        fecha_creacion = DateTime.Now,
                        fecha_modificacion = DateTime.Now
                    });
                }
                await _db.SaveChangesAsync();
                return Json(new { ok = true, msg = "Guardado correctamente" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarEstado(int id)
        {
            try
            {
                var est = await _db.EstadosVenta.FindAsync(id);
                if (est != null) { est.estado = false; est.fecha_modificacion = DateTime.Now; }
                await _db.SaveChangesAsync();
                return Json(new { ok = true, msg = "Estado desactivado" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }

    // ── Request Models ────────────────────────────────────────────────────────
    public class ProductoPOSItem
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio_unitario { get; set; }
        public decimal descuento { get; set; }
    }

    public class RegistrarPOSRequest
    {
        public int id_cliente { get; set; }
        public int id_caja { get; set; }
        public int id_almacen { get; set; }
        public int? id_cotizacion { get; set; }
        public string tipo_comprobante { get; set; } = "Boleta";
        public string metodo_pago { get; set; } = "Efectivo";
        public string? tipo_documento_cli { get; set; }
        public string? numero_documento_cli { get; set; }
        public decimal descuento { get; set; } = 0;
        public decimal monto_pagado { get; set; }
        public List<ProductoPOSItem> productos { get; set; } = new();
    }

    public class ProductoCotizacionItem
    {
        public int id_producto { get; set; }
        public int cantidad { get; set; }
        public decimal precio_unitario { get; set; }
        public decimal descuento { get; set; }
    }

    public class CrearCotizacionRequest
    {
        public int id_cliente { get; set; }
        public string? observacion { get; set; }
        public List<ProductoCotizacionItem> productos { get; set; } = new();
    }
}