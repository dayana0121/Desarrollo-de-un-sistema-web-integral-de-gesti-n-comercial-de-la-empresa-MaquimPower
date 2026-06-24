using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using E1.Models;
using E1.Helpers;

namespace E1.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string _conn;
        private readonly IConfiguration _config;
        public DashboardController(IConfiguration config)
        {
            _conn = config.GetConnectionString("MaquimPowerDB")!;
            _config = config;
        }

        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            // Verificar acceso al módulo Dashboard (Admin siempre pasa)
            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Dashboard", "Dashboard"))
                return RedirectToAction("SinAcceso", "Home");
            using var db = new SqlConnection(_conn);

            // sp_Dashboard_Stats devuelve 5 resultsets
            using var multi = db.QueryMultiple("EXEC sp_Dashboard_Stats");

            var model = new Dashboard
            {
                VentasHoy = multi.ReadFirstOrDefault<decimal>(),
                TotalProductos = multi.ReadFirstOrDefault<int>(),
                TotalClientes = multi.ReadFirstOrDefault<int>(),
                StockCritico = multi.ReadFirstOrDefault<int>(),
                VentasMensuales = multi.Read<VentaMensual>().ToList(),
                TopProductos = multi.Read<ProductoTop>().ToList()
            };

            return View(model);
        }
    }
}