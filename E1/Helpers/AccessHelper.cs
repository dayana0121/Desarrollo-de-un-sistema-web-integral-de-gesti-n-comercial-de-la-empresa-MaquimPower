using Microsoft.Data.SqlClient;
using Dapper;

namespace E1.Helpers
{
    /// <summary>
    /// Helper para verificar accesos por rol.
    /// El Administrador (es_sistema = 1) siempre tiene acceso total sin restricciones.
    /// </summary>
    public static class AccessHelper
    {
        // Clave en sesión donde guardamos los accesos cacheados (CSV: "Modulo|Submodulo,...")
        private const string SESSION_KEY = "AccesosRol";

        /// <summary>
        /// Verifica si el usuario en sesión tiene acceso al módulo/submódulo dado.
        /// Si el rol es de sistema (Administrador), devuelve true siempre.
        /// </summary>
        public static bool TieneAcceso(ISession session, IConfiguration config,
                                       string modulo, string submodulo)
        {
            // Administrador (es_sistema = 1) tiene acceso total — nadie se la quita
            var idRol = SessionHelper.GetIdRol(session);
            if (idRol == null) return false;

            if (EsRolSistema(session, config, idRol.Value))
                return true;

            var accesos = ObtenerAccesos(session, config, idRol.Value);
            return accesos.Contains($"{modulo}|{submodulo}", StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene lista de "Modulo|Submodulo" permitidos para el rol actual.
        /// Usa caché en sesión para evitar queries repetidos.
        /// </summary>
        public static IList<string> ObtenerAccesos(ISession session, IConfiguration config, int idRol)
        {
            // Intentar leer del caché de sesión
            var cached = session.GetString(SESSION_KEY);
            if (cached != null)
                return cached.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Query a la BD
            var conn = config.GetConnectionString("MaquimPowerDB")!;
            using var db = new SqlConnection(conn);
            var rows = db.Query("EXEC sp_AccesosPorRol_ObtenerPorRol @id_rol", new { id_rol = idRol });

            var lista = rows
                .Where(r => r.tiene_acceso == true)
                .Select(r => $"{r.modulo}|{r.submodulo}")
                .ToList<string>();

            // Guardar en sesión (TTL controlado por la sesión misma)
            session.SetString(SESSION_KEY, string.Join(",", lista));
            return lista;
        }

        /// <summary>
        /// Limpia el caché de accesos de la sesión (llamar al cambiar rol o al logout).
        /// </summary>
        public static void LimpiarCache(ISession session)
        {
            session.Remove(SESSION_KEY);
        }

        /// <summary>
        /// Comprueba si el rol del usuario actual es de sistema (Administrador).
        /// </summary>
        private static bool EsRolSistema(ISession session, IConfiguration config, int idRol)
        {
            // Guardamos un flag en sesión para no re-consultar cada vez
            var flag = session.GetString("EsSistema");
            if (flag != null)
                return flag == "1";

            var conn = config.GetConnectionString("MaquimPowerDB")!;
            using var db = new SqlConnection(conn);
            var esSistema = db.ExecuteScalar<bool>(
                "SELECT ISNULL(es_sistema, 0) FROM Roles WHERE id_rol = @id_rol",
                new { id_rol = idRol });

            session.SetString("EsSistema", esSistema ? "1" : "0");
            return esSistema;
        }
    }
}
