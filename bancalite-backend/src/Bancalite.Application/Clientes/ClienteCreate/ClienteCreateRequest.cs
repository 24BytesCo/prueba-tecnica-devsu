using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bancalite.Application.Clientes.ClienteCreate
{
    /// <summary>
    /// Solicitud para crear un nuevo cliente (y su persona asociada).
    /// </summary>
    public class ClienteCreateRequest
    {
        /// <summary>
        /// Nombres de la persona.
        /// </summary>
        public string? Nombres { get; set; }

        /// <summary>
        /// Apellidos de la persona.
        /// </summary>
        public string? Apellidos { get; set; }

        /// <summary>
        /// Edad de la persona.
        /// </summary>
        public int Edad { get; set; }

        /// <summary>
        /// Id del género (FK a catálogo Generos).
        /// </summary>
        public Guid? GeneroId { get; set; }

        /// <summary>
        /// Id del tipo de documento (FK a catálogo TiposDocumentoIdentidad).
        /// </summary>
        public Guid TipoDocumentoIdentidad { get; set; }

        /// <summary>
        /// Número de documento de identidad.
        /// </summary>
        public string? NumeroDocumento { get; set; }

        /// <summary>
        /// Dirección de residencia (opcional).
        /// </summary>
        public string? Direccion { get; set; }

        /// <summary>
        /// Teléfono de contacto (opcional).
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Correo electrónico (obligatorio). Se usa para iniciar sesión.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Contraseña del usuario a crear (se almacenará el hash en Cliente).
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Id de rol (opcional). Autorización se maneja con Identity.
        /// </summary>
        public Guid? IdRol { get; set; }
    }
}
