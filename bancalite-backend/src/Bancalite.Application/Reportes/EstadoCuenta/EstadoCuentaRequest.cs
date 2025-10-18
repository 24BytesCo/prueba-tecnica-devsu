namespace Bancalite.Application.Reportes.EstadoCuenta
{
    /// <summary>
    /// Filtros del reporte de estado de cuenta.
    /// </summary>
    public class EstadoCuentaRequest
    {
        /// <summary>
        /// Identificador del cliente (opcional si se indica número de cuenta).
        /// </summary>
        public Guid? ClienteId { get; set; }

        /// <summary>
        /// Número de cuenta (opcional si se indica cliente).
        /// </summary>
        public string? NumeroCuenta { get; set; }

        /// <summary>
        /// Fecha desde (incluyente, UTC).
        /// </summary>
        public DateTime Desde { get; set; }

        /// <summary>
        /// Fecha hasta (incluyente, UTC).
        /// </summary>
        public DateTime Hasta { get; set; }
    }
}

