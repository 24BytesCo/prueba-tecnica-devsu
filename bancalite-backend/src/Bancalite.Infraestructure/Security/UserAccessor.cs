using System.Security.Claims;
using Bancalite.Application.Interface;
using Microsoft.AspNetCore.Http;

namespace Bancalite.Infraestructure.Security
{
    /// <summary>
    /// Acceso al usuario actual desde el contexto HTTP.
    /// </summary>
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Crea una nueva instancia del UserAccessor.
        /// </summary>
        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Obtiene un identificador del usuario autenticado (prioriza Email).
        /// </summary>
        /// <returns>Nombre de usuario o cadena vacía si no autenticado.</returns>
        public string GetUsername()
        {
            // Se prioriza el Email; si no hay, se intenta con NameIdentifier; luego Name
            var user = _httpContextAccessor.HttpContext?.User; // usuario actual
            if (user?.Identity?.IsAuthenticated != true) return string.Empty; // no autenticado

            var email = user.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email)) return email;

            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(id)) return id;

            var name = user.FindFirstValue(ClaimTypes.Name);
            return name ?? string.Empty;
        }

        /// <summary>
        /// Verifica si el usuario autenticado actual pertenece a un rol.
        /// </summary>
        public bool IsInRole(string role)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true && user.IsInRole(role);
        }
    }
}
