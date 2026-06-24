using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using E1.Models;
using E1.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace E1.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly string _conn;
        private readonly IConfiguration _config;
        public SeguridadController(IConfiguration config)
        {
            _conn = config.GetConnectionString("MaquimPowerDB")!;
            _config = config;
        }

        // ── Tab Roles ────────────────────────────────────────
        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Seguridad", "Roles"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var roles = db.Query<Rol>("EXEC sp_Roles_Listar").ToList();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public IActionResult CrearRol(Rol model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Roles_Crear @nombre", model);
            return Json(new { ok = true, msg = "Rol creado" });
        }

        [HttpPost]
        public IActionResult ActualizarRol(Rol model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Roles_Actualizar @id_rol, @nombre, @estado", model);
            return Json(new { ok = true, msg = "Rol actualizado" });
        }

        [HttpPost]
        public IActionResult EliminarRol(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            var result = db.QueryFirstOrDefault<dynamic>(
                "EXEC sp_Roles_Eliminar @id_rol", new { id_rol = id });

            if (result?.resultado == -1)
                return Json(new { ok = false, msg = (string)result.mensaje });

            return Json(new { ok = true, msg = "Rol eliminado" });
        }

        // ── Tab Permisos ─────────────────────────────────────
        public IActionResult Permisos(string? busqueda)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Seguridad", "Permisos"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var permisos = db.Query<Permiso>(@"
                SELECT * FROM Permisos
                WHERE (@busqueda IS NULL OR nombre LIKE '%'+@busqueda+'%'
                    OR modulo LIKE '%'+@busqueda+'%')
                ORDER BY modulo, accion",
                new { busqueda }).ToList();

            ViewBag.Permisos = permisos;
            ViewBag.Busqueda = busqueda;
            return View();
        }

        [HttpPost]
        public IActionResult CrearPermiso(Permiso model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute(@"INSERT INTO Permisos (nombre,codigo,modulo,accion)
                VALUES (@nombre,@codigo,@modulo,@accion)", model);
            return Json(new { ok = true, msg = "Permiso creado" });
        }

        [HttpPost]
        public IActionResult ActualizarPermiso(Permiso model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute(@"UPDATE Permisos SET nombre=@nombre, codigo=@codigo,
                modulo=@modulo, accion=@accion, estado=@estado,
                fecha_modificacion=GETDATE()
                WHERE id_permiso=@id_permiso", model);
            return Json(new { ok = true, msg = "Permiso actualizado" });
        }

        [HttpPost]
        public IActionResult EliminarPermiso(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute("UPDATE Permisos SET estado=0, fecha_modificacion=GETDATE() WHERE id_permiso=@id",
                new { id });
            return Json(new { ok = true, msg = "Permiso desactivado" });
        }

        // ── Tab Accesos por Rol ──────────────────────────────
        public IActionResult AccesosPorRol(int? id_rol)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Seguridad", "Accesos"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var roles = db.Query<Rol>("EXEC sp_Roles_Listar").ToList();
            ViewBag.Roles = roles;

            if (id_rol.HasValue)
            {
                var accesos = db.Query<AccesoPorRol>(
                    "EXEC sp_AccesosPorRol_ObtenerPorRol @id_rol",
                    new { id_rol }).ToList();
                ViewBag.Accesos = accesos;
                ViewBag.IdRolSel = id_rol;
            }

            return View();
        }

        [HttpPost]
        public IActionResult GuardarAcceso(int id_rol, string modulo, string submodulo, bool tiene_acceso)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_AccesosPorRol_Guardar @id_rol, @modulo, @submodulo, @tiene_acceso",
                new { id_rol, modulo, submodulo, tiene_acceso });
            return Json(new { ok = true });
        }

        // Guardar todos los accesos de un rol de una vez (desde la matriz de checkboxes)
        [HttpPost]
        public IActionResult GuardarTodosAccesos(int id_rol, [FromBody] List<AccesoPorRol> accesos)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            foreach (var a in accesos)
            {
                db.Execute("EXEC sp_AccesosPorRol_Guardar @id_rol, @modulo, @submodulo, @tiene_acceso",
                    new { id_rol, a.modulo, a.submodulo, a.tiene_acceso });
            }
            return Json(new { ok = true, msg = "Accesos guardados" });
        }

        // ── Tab Usuarios ─────────────────────────────────────
        public IActionResult Usuarios(string? busqueda)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Seguridad", "Usuarios"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var usuarios = db.Query<dynamic>(
                "EXEC sp_Usuarios_Listar @busqueda", new { busqueda }).ToList();
            var roles = db.Query<Rol>("EXEC sp_Roles_Listar").ToList();

            ViewBag.Usuarios = usuarios;
            ViewBag.Roles = roles;
            ViewBag.Busqueda = busqueda;
            return View();
        }

        [HttpPost]
        public IActionResult CrearUsuario(Usuario model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            model.password_hash = HashSHA256(model.password_hash); // viene la contraseña en texto plano

            using var db = new SqlConnection(_conn);
            var result = db.QueryFirstOrDefault<dynamic>(
                "EXEC sp_Usuarios_Crear @id_rol, @nombre, @username, @password_hash", model);

            if (result?.resultado == -1)
                return Json(new { ok = false, msg = (string)result.mensaje });

            return Json(new { ok = true, msg = "Usuario creado" });
        }

        [HttpPost]
        public IActionResult ActualizarUsuario(Usuario model, string? nueva_password)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            string? hash = string.IsNullOrEmpty(nueva_password) ? null : HashSHA256(nueva_password);

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Usuarios_Actualizar @id_usuario, @id_rol, @nombre, @username, @password_hash, @bloqueado, @estado",
                new
                {
                    model.id_usuario,
                    model.id_rol,
                    model.nombre,
                    model.username,
                    password_hash = hash,
                    model.bloqueado,
                    model.estado
                });
            return Json(new { ok = true, msg = "Usuario actualizado" });
        }

        [HttpPost]
        public IActionResult EliminarUsuario(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Usuarios_Eliminar @id_usuario", new { id_usuario = id });
            return Json(new { ok = true, msg = "Usuario desactivado" });
        }

        private static string HashSHA256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}