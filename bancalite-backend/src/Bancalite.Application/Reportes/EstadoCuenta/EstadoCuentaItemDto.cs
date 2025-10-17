using System;

namespace Bancalite.Application.Reportes.EstadoCuenta
{
    /// <summary>
    /// Item de detalle de movimiento para el reporte de estado de cuenta.
    /// </summary>
    public class EstadoCuentaItemDto
    {
        public DateTime Fecha { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public string TipoCodigo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal SaldoPrevio { get; set; }
        public decimal SaldoPosterior { get; set; }
        public string? Descripcion { get; set; }
    }
}

