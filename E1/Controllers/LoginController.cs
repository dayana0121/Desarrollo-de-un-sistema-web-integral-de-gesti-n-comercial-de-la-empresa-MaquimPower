using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Security.Cryptography;
using System.Text;
using E1.Models;
using E1.Helpers;

namespace E1.Controllers
{
    public class LoginController : Controller
    {
        private readonly string _conn;
        public LoginController(IConfiguration config)
        {
            _conn = config.GetConnectionString("MaquimPowerDB")!;
        }

        // GET: /Login
        public IActionResult Index()
        {
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        // POST: /Login
        [HttpPost]
        public IActionResult Index(Login model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var hash = HashSHA256(model.password);

            using var db = new SqlConnection(_conn);
            var usuario = db.QueryFirstOrDefault(
                "EXEC sp_Login @username, @password_hash",
                new { username = model.username, password_hash = hash }
            );

            if (usuario == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                return View(model);
            }

            SessionHelper.SetUsuario(
                HttpContext.Session,
                (int)usuario.id_usuario,
                (string)usuario.nombre,
                (string)usuario.username,
                (int)usuario.id_rol,
                (string)usuario.nombre_rol
            );

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Login/Logout
        public IActionResult Logout()
        {
            AccessHelper.LimpiarCache(HttpContext.Session);
            SessionHelper.ClearSession(HttpContext.Session);
            return RedirectToAction("Index", "Home");
        }

        private static string HashSHA256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}