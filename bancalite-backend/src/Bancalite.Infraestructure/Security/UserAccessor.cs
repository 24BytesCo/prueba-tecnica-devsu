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
        /// Obtiene el nombre de usuario del usuario autenticado.
        /// </summary>
        /// <returns>Nombre de usuario o cadena vacía si no autenticado.</returns>
        public string GetUsername()
        {
            // Toma el claim Name; si no hay, intenta con Email; si no, vacío
            var user = _httpContextAccessor.HttpContext?.User; // usuario actual
            if (user?.Identity?.IsAuthenticated != true) return string.Empty; // no autenticado

            var name = user.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrWhiteSpace(name)) return name;

            var email = user.FindFirstValue(ClaimTypes.Email);
            return email ?? string.Empty;
        }
    }
}
