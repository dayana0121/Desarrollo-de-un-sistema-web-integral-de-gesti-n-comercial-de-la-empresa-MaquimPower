using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using E1.Models;
using E1.Helpers;

namespace E1.Controllers
{
    public class ProductosController : Controller
    {
        private readonly string _conn;
        private readonly IConfiguration _config;

        public ProductosController(IConfiguration config)
        {
            _conn = config.GetConnectionString("MaquimPowerDB")!;
            _config = config;
        }

        // ── Tab Productos ────────────────────────────────────
        public IActionResult Index(string? busqueda, int? id_categoria, int? id_marca, int soloActivos = 1)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var productos = db.Query<dynamic>(
                "EXEC sp_Productos_Listar @busqueda, @id_categoria, @id_marca, @solo_activos",
                new { busqueda, id_categoria, id_marca, solo_activos = soloActivos }
            ).ToList();

            var categorias = db.Query<Categoria>("EXEC sp_Categorias_Listar @busqueda=NULL, @solo_activos=1").ToList();
            var marcas = db.Query<Marca>("EXEC sp_Marcas_Listar @busqueda=NULL, @solo_activos=1").ToList();
            var proveedores = db.Query<Proveedor>("EXEC sp_Proveedores_Listar @busqueda=NULL, @solo_activos=1").ToList();

            ViewBag.Productos = productos;
            ViewBag.Categorias = categorias;
            ViewBag.Marcas = marcas;
            ViewBag.Proveedores = proveedores;
            ViewBag.Busqueda = busqueda;
            ViewBag.FiltroCategoria = id_categoria;
            ViewBag.FiltroMarca = id_marca;

            return View();
        }

        [HttpPost]
        public IActionResult CrearProducto(Producto model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            var result = db.QueryFirstOrDefault<dynamic>(
                "EXEC sp_Productos_Crear @id_categoria, @id_marca, @id_proveedor, @sku, @nombre, @descripcion, @precio_costo, @precio_venta, @stock_minimo",
                model);

            if (result?.resultado == -1)
                return Json(new { ok = false, msg = (string)result.mensaje });

            return Json(new { ok = true, msg = "Producto creado correctamente" });
        }

        [HttpPost]
        public IActionResult ActualizarProducto(Producto model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Productos_Actualizar @id_producto, @id_categoria, @id_marca, @id_proveedor, @sku, @nombre, @descripcion, @precio_costo, @precio_venta, @stock_minimo, @estado", model);
            return Json(new { ok = true, msg = "Producto actualizado" });
        }

        [HttpPost]
        public IActionResult EliminarProducto(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Productos_Eliminar @id_producto", new { id_producto = id });
            return Json(new { ok = true, msg = "Producto desactivado" });
        }

        // ── Tab Categorías ───────────────────────────────────
        public IActionResult Categorias(string? busqueda, int soloActivos = 1)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var categorias = db.Query<Categoria>(
                "EXEC sp_Categorias_Listar @busqueda, @solo_activos",
                new { busqueda, solo_activos = soloActivos }
            ).ToList();

            ViewBag.Categorias = categorias;
            ViewBag.Busqueda = busqueda;
            return View();
        }

        [HttpPost]
        public IActionResult CrearCategoria(Categoria model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Categorias_Crear @nombre, @codigo, @descripcion", model);
            return Json(new { ok = true, msg = "Categoría creada" });
        }

        [HttpPost]
        public IActionResult ActualizarCategoria(Categoria model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Categorias_Actualizar @id_categoria, @nombre, @codigo, @descripcion, @estado", model);
            return Json(new { ok = true, msg = "Categoría actualizada" });
        }

        [HttpPost]
        public IActionResult EliminarCategoria(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Categorias_Eliminar @id_categoria", new { id_categoria = id });
            return Json(new { ok = true, msg = "Categoría desactivada" });
        }

        // ── Tab Marcas ───────────────────────────────────────
        public IActionResult Marcas(string? busqueda, int soloActivos = 1)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Index", "Login");

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return RedirectToAction("SinAcceso", "Home");

            using var db = new SqlConnection(_conn);
            var marcas = db.Query<Marca>(
                "EXEC sp_Marcas_Listar @busqueda, @solo_activos",
                new { busqueda, solo_activos = soloActivos }
            ).ToList();

            ViewBag.Marcas = marcas;
            ViewBag.Busqueda = busqueda;
            return View();
        }

        [HttpPost]
        public IActionResult CrearMarca(Marca model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Marcas_Crear @nombre, @codigo", model);
            return Json(new { ok = true, msg = "Marca creada" });
        }

        [HttpPost]
        public IActionResult ActualizarMarca(Marca model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Marcas_Actualizar @id_marca, @nombre, @codigo, @estado", model);
            return Json(new { ok = true, msg = "Marca actualizada" });
        }

        [HttpPost]
        public IActionResult EliminarMarca(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new { ok = false, msg = "Sesión expirada" });

            if (!AccessHelper.TieneAcceso(HttpContext.Session, _config, "Productos", "Productos"))
                return Json(new { ok = false, msg = "No tiene permisos para esta operación" });

            using var db = new SqlConnection(_conn);
            db.Execute("EXEC sp_Marcas_Eliminar @id_marca", new { id_marca = id });
            return Json(new { ok = true, msg = "Marca desactivada" });
        }
    }
}