using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using E1.Models;
using E1.Helpers;

namespace E1.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _conn;
        public HomeController(IConfiguration config)
        {
            _conn = config.GetConnectionString("MaquimPowerDB")!;
        }

        public IActionResult Index()
        {
            if (SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Dashboard");

            using var db = new SqlConnection(_conn);
            var empresa = db.QueryFirstOrDefault<Empresa>("SELECT TOP 1 * FROM Empresa");
            return View(empresa);
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult SinAcceso()
        {
            ViewData["Title"] = "Acceso Denegado";
            return View();
        }
    }
}
