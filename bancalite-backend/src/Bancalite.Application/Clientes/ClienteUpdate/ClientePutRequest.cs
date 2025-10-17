using System;

namespace Bancalite.Application.Clientes.ClienteUpdate
{
    /// <summary>
    /// Solicitud para actualización total (PUT) del cliente y su persona.
    /// </summary>
    public class ClientePutRequest
    {
        /// <summary>
        /// Nombres de la persona.
        /// </summary>
        public string Nombres { get; set; } = null!;

        /// <summary>
        /// Apellidos de la persona.
        /// </summary>
        public string Apellidos { get; set; } = null!;

        /// <summary>
        /// Edad de la persona.
        /// </summary>
        public int Edad { get; set; }

        /// <summary>
        /// Identificador del género.
        /// </summary>
        public Guid GeneroId { get; set; }

        /// <summary>
        /// Identificador del tipo de documento de identidad.
        /// </summary>
        public Guid TipoDocumentoIdentidadId { get; set; }

        /// <summary>
        /// Número de documento de identidad (único por tipo).
        /// </summary>
        public string NumeroDocumento { get; set; } = null!;

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
        public bool Estado { get; set; } = true;
    }
}
