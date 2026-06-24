using Microsoft.AspNetCore.Http;

namespace E1.Helpers
{
    public static class SessionHelper
    {
        // Guardar datos al hacer login
        public static void SetUsuario(ISession session, int idUsuario, string nombre, string username, int idRol, string nombreRol)
        {
            session.SetInt32("IdUsuario", idUsuario);
            session.SetString("Nombre", nombre);
            session.SetString("Username", username);
            session.SetInt32("IdRol", idRol);
            session.SetString("NombreRol", nombreRol);
        }

        // Obtener datos de sesión
        public static int? GetIdUsuario(ISession session) => session.GetInt32("IdUsuario");
        public static string? GetNombre(ISession session) => session.GetString("Nombre");
        public static string? GetUsername(ISession session) => session.GetString("Username");
        public static int? GetIdRol(ISession session) => session.GetInt32("IdRol");
        public static string? GetNombreRol(ISession session) => session.GetString("NombreRol");

        // Verificar si hay sesión activa
        public static bool IsLoggedIn(ISession session) => session.GetInt32("IdUsuario").HasValue;

        // Cerrar sesión
        public static void ClearSession(ISession session) => session.Clear();
    }
}