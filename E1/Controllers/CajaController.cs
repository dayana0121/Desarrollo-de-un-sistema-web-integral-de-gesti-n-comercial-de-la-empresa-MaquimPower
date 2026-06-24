using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using E1.Data;
using E1.Models;

namespace E1.Controllers
{
    public class CajaController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;

        public CajaController(AppDbContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }

        private int GetIdUsuario()
        {
            return _http.HttpContext!.Session.GetInt32("IdUsuario") ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return RedirectToAction("Index", "Login");

            var cajaAbierta = await _db.Cajas
                .Where(c => c.id_usuario == idUsuario && c.estado == "Abierta")
                .FirstOrDefaultAsync();

            List<MovimientoCaja> movimientos = new();
            if (cajaAbierta != null)
            {
                movimientos = await _db.MovimientosCaja
                    .Where(m => m.id_caja == cajaAbierta.id_caja)
                    .OrderByDescending(m => m.fecha_creacion)
                    .ToListAsync();
            }

            ViewBag.CajaAbierta = cajaAbierta;
            ViewBag.Movimientos = movimientos;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Abrir(string nombre_caja, decimal monto_apertura)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return Json(new { ok = false, msg = "Sesión expirada" });
            try
            {
                using var conn = new SqlConnection(_db.Database.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_Cajas_Abrir", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@nombre_caja", nombre_caja);
                cmd.Parameters.AddWithValue("@monto_apertura", monto_apertura);
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

        [HttpPost]
        public async Task<IActionResult> Cerrar(int id_caja, decimal monto_cierre, string? observacion)
        {
            try
            {
                using var conn = new SqlConnection(_db.Database.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_Cajas_Cerrar", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_caja", id_caja);
                cmd.Parameters.AddWithValue("@monto_cierre", monto_cierre);
                cmd.Parameters.AddWithValue("@observacion", (object?)observacion ?? DBNull.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int resultado = Convert.ToInt32(reader["resultado"]);
                    string mensaje = reader["mensaje"].ToString()!;
                    decimal esperado = reader["monto_esperado"] != DBNull.Value ? Convert.ToDecimal(reader["monto_esperado"]) : 0;
                    decimal diferencia = reader["diferencia"] != DBNull.Value ? Convert.ToDecimal(reader["diferencia"]) : 0;
                    return Json(new { ok = resultado > 0, msg = mensaje, esperado, diferencia });
                }
                return Json(new { ok = false, msg = "Sin respuesta" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarMovimiento(int id_caja, string tipo, string concepto, decimal monto)
        {
            int idUsuario = GetIdUsuario();
            if (idUsuario == 0) return Json(new { ok = false, msg = "Sesión expirada" });
            try
            {
                using var conn = new SqlConnection(_db.Database.GetConnectionString());
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_Cajas_RegistrarMovimiento", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_caja", id_caja);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@concepto", concepto);
                cmd.Parameters.AddWithValue("@monto", monto);
                cmd.Parameters.AddWithValue("@tipo_referencia", DBNull.Value);
                cmd.Parameters.AddWithValue("@id_referencia", DBNull.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int resultado = Convert.ToInt32(reader["resultado"]);
                    return Json(new { ok = resultado > 0, msg = resultado > 0 ? "Movimiento registrado" : "Error" });
                }
                return Json(new { ok = false, msg = "Sin respuesta" });
            }
            catch (Exception ex) { return Json(new { ok = false, msg = ex.Message }); }
        }
    }
}