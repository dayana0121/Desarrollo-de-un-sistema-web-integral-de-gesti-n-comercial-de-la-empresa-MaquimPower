using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using E1.Models;
using E1.Helpers;

namespace E1.Controllers
{
    public class ConfiguracionController : Controller
    {
        private readonly string _conn;
        private readonly IConfiguration _config;
        public ConfiguracionController(IConfiguration config)
        {
            _conn = config.GetConnectionString("MaquimPowerDB")!;
            _config = config;
        }

        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Configuracion", "Configuracion"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var empresa = db.QueryFirstOrDefault<Empresa>("SELECT TOP 1 * FROM Empresa");
            return View(empresa);
        }

        [HttpPost]
        public IActionResult Guardar(Empresa model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Configuracion", "Configuracion"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);

            // Si ya existe la empresa la actualizamos, si no la insertamos
            var existe = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Empresa");
            if (existe > 0)
            {
                db.Execute(@"UPDATE Empresa SET
                    nombre=@nombre, ruc=@ruc, direccion=@direccion,
                    telefono=@telefono, email=@email, logo_url=@logo_url,
                    fecha_modificacion=GETDATE()
                    WHERE id_empresa = (SELECT TOP 1 id_empresa FROM Empresa)", model);
            }
            else
            {
                db.Execute(@"INSERT INTO Empresa (nombre,ruc,direccion,telefono,email,logo_url)
                    VALUES (@nombre,@ruc,@direccion,@telefono,@email,@logo_url)", model);
            }

            TempData["Mensaje"] = "Configuración guardada correctamente";
            return RedirectToAction("Index");
        }
    }
}