namespace Bancalite.Application.Config
{
    /// <summary>
    /// Opciones del módulo Movimientos.
    /// </summary>
    public class MovimientosOptions
    {
        /// <summary>
        /// Tope diario de retiros por cuenta (solo débitos). Default: 1000.00
        /// </summary>
        public decimal TopeDiario { get; set; } = 1000m;

        /// <summary>
        /// Modo de redondeo monetario. Por defecto Bankers (Half-Even).
        /// </summary>
        public System.MidpointRounding RoundingMode { get; set; } = System.MidpointRounding.ToEven;
    }
}

