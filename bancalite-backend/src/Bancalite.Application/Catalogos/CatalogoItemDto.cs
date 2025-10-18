using System;

namespace Bancalite.Application.Catalogos
{
    /// <summary>
    /// DTO compacto para elementos de catálogo (Id, Código y Nombre).
    /// </summary>
    public class CatalogoItemDto
    {
        /// <summary>
        /// Identificador del elemento de catálogo.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Código corto del elemento (ej: CC, M).
        /// </summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo del elemento.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Indicador de si el elemento está activo.
        /// </summary>
        public bool Activo { get; set; }
    }
}

