namespace Bancalite.Application.Clientes.ClienteList
{
    /// <summary>
    /// Elemento de salida para listar clientes.
    /// </summary>
    public class ClienteListItem
    {
        /// <summary>
        /// Id del cliente.
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Id de la persona asociada.
        /// </summary>
        public Guid PersonaId { get; set; }

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
        /// Id del género de la persona.
        /// </summary>
        public Guid GeneroId { get; set; }

        /// <summary>
        /// Nombre del género.
        /// </summary>
        public string GeneroNombre { get; set; } = null!;

        /// <summary>
        /// Código del género.
        /// </summary>
        public string GeneroCodigo { get; set; } = null!;

        /// <summary>
        /// Id del tipo de documento.
        /// </summary>
        public Guid TipoDocumentoIdentidadId { get; set; }

        /// <summary>
        /// Nombre del tipo de documento de identidad.
        /// </summary>
        public string TipoDocumentoIdentidadNombre { get; set; } = null!;

        /// <summary>
        /// Código del tipo de documento de identidad.
        /// </summary>
        public string TipoDocumentoIdentidadCodigo { get; set; } = null!;

        /// <summary>
        /// Número de documento.
        /// </summary>
        public string NumeroDocumento { get; set; } = null!;

        /// <summary>
        /// Dirección (opcional).
        /// </summary>
        public string? Direccion { get; set; }

        /// <summary>
        /// Teléfono (opcional).
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Email (opcional).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Estado del cliente (activo/inactivo).
        /// </summary>
        public bool Estado { get; set; }

        /// <summary>
        /// Id del rol asignado al usuario (si existe vínculo con Identity).
        /// </summary>
        public Guid? RolId { get; set; }

        /// <summary>
        /// Nombre del rol (si existe).
        /// </summary>
        public string? RolNombre { get; set; }
    }
}
