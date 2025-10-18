using System;
using System.Text.Json.Serialization;

namespace Bancalite.Application.Clientes.ClienteUpdate
{
    /// <summary>
    /// Solicitud para actualización parcial (PATCH) del cliente y/o su persona.
    /// Solo los campos con valor serán actualizados.
    /// </summary>
    public class ClientePatchRequest
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
        public int? Edad { get; set; }

        /// <summary>
        /// Identificador del género.
        /// </summary>
        public Guid? GeneroId { get; set; }

        /// <summary>
        /// Identificador del tipo de documento de identidad.
        /// </summary>
        public Guid? TipoDocumentoIdentidadId { get; set; }

        /// <summary>
        /// Alias de compatibilidad para clientes que envían "tipoDocumentoIdentidad" (sin sufijo Id).
        /// Asigna al campo Id cuando está presente.
        /// </summary>
        [JsonPropertyName("tipoDocumentoIdentidad")]
        public Guid? TipoDocumentoIdentidad
        {
            set => TipoDocumentoIdentidadId = value;
        }

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
        /// Correo electrónico (opcional).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Estado del cliente (activo/inactivo).
        /// </summary>
        public bool? Estado { get; set; }
    }
}
