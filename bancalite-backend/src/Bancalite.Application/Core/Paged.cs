namespace Bancalite.Application.Core
{
    /// <summary>
    /// Parametros de paginación básicos.
    /// </summary>
    public class PageParams
    {
        /// <summary>
        /// Página actual (1-based).
        /// </summary>
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página.
        /// </summary>
        public int Tamano { get; set; } = 10;
    }

    /// <summary>
    /// Resultado paginado con metadatos.
    /// </summary>
    public class Paged<T>
    {
        /// <summary>
        /// Colección de elementos de la página actual.
        /// </summary>
        public required IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

        /// <summary>
        /// Total de elementos en toda la colección (sin paginar).
        /// </summary>
        public int Total { get; init; }

        /// <summary>
        /// Número de página actual (1-based).
        /// </summary>
        public int Pagina { get; init; }

        /// <summary>
        /// Tamaño de página utilizado.
        /// </summary>
        public int Tamano { get; init; }
    }
}
