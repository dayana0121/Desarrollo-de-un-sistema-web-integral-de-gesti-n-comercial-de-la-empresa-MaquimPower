using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using E1.Models;
using System.Data;

namespace E1.Controllers
{
    public class TercerosController : Controller
    {
        private readonly string _conn;

        public TercerosController(IConfiguration configuration)
        {
            _conn = configuration.GetConnectionString("MaquimPowerDB")!;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Clientes));
        }

        public IActionResult Clientes()
        {
            return View();
        }

        public IActionResult Proveedores()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ListarClientes()
        {
            using var db = new SqlConnection(_conn);

            var data = db.Query<Cliente>(
                "sp_Clientes_Listar",
                new { busqueda = (string?)null, solo_activos = false },
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Json(new { data });
        }

        [HttpPost]
        public IActionResult GuardarCliente(Cliente cliente)
        {
            using var db = new SqlConnection(_conn);

            if (cliente.id_cliente == 0)
            {
                db.Execute("sp_Clientes_Crear", new
                {
                    cliente.tipo_documento,
                    cliente.numero_documento,
                    cliente.nombre_completo,
                    cliente.telefono,
                    cliente.direccion,
                    cliente.email
                }, commandType: CommandType.StoredProcedure);
            }
            else
            {
                db.Execute("sp_Clientes_Actualizar", new
                {
                    cliente.id_cliente,
                    cliente.tipo_documento,
                    cliente.numero_documento,
                    cliente.nombre_completo,
                    cliente.telefono,
                    cliente.direccion,
                    cliente.email,
                    cliente.estado
                }, commandType: CommandType.StoredProcedure);
            }

            return Json(new { ok = true, mensaje = "Cliente guardado correctamente" });
        }

        [HttpPost]
        public IActionResult EliminarCliente(int id_cliente)
        {
            using var db = new SqlConnection(_conn);

            db.Execute(
                "sp_Clientes_Eliminar",
                new { id_cliente },
                commandType: CommandType.StoredProcedure
            );

            return Json(new { ok = true, mensaje = "Cliente eliminado correctamente" });
        }

        [HttpGet]
        public IActionResult ListarProveedores()
        {
            using var db = new SqlConnection(_conn);

            var data = db.Query<Proveedor>(
                "sp_Proveedores_Listar",
                new { busqueda = (string?)null, solo_activos = false },
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Json(new { data });
        }

        [HttpPost]
        public IActionResult GuardarProveedor(Proveedor proveedor)
        {
            using var db = new SqlConnection(_conn);

            if (proveedor.id_proveedor == 0)
            {
                db.Execute("sp_Proveedores_Crear", new
                {
                    proveedor.ruc,
                    proveedor.razon_social,
                    proveedor.nombre_contacto,
                    proveedor.telefono,
                    proveedor.email,
                    proveedor.direccion
                }, commandType: CommandType.StoredProcedure);
            }
            else
            {
                db.Execute("sp_Proveedores_Actualizar", new
                {
                    proveedor.id_proveedor,
                    proveedor.ruc,
                    proveedor.razon_social,
                    proveedor.nombre_contacto,
                    proveedor.telefono,
                    proveedor.email,
                    proveedor.direccion,
                    proveedor.estado
                }, commandType: CommandType.StoredProcedure);
            }

            return Json(new { ok = true, mensaje = "Proveedor guardado correctamente" });
        }

        [HttpPost]
        public IActionResult EliminarProveedor(int id_proveedor)
        {
            using var db = new SqlConnection(_conn);

            db.Execute(
                "sp_Proveedores_Eliminar",
                new { id_proveedor },
                commandType: CommandType.StoredProcedure
            );

            return Json(new { ok = true, mensaje = "Proveedor eliminado correctamente" });
        }
    }
}