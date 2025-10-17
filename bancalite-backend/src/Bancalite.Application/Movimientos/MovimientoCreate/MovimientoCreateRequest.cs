using System;

namespace Bancalite.Application.Movimientos.MovimientoCreate
{
    /// <summary>
    /// Datos para registrar un movimiento (Crédito/Débito).
    /// </summary>
    public class MovimientoCreateRequest
    {
        /// <summary>
        /// Número de cuenta destino del movimiento.
        /// </summary>
        public string NumeroCuenta { get; set; } = string.Empty;

        /// <summary>
        /// Código del tipo de movimiento: CRE (crédito) o DEB (débito).
        /// </summary>
        public string TipoCodigo { get; set; } = string.Empty; // CRE/DEB

        /// <summary>
        /// Monto original (positivo). Se normaliza a 2 decimales con redondeo bancario.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Clave de idempotencia opcional. Si se repite, devuelve el mismo resultado sin duplicar.
        /// </summary>
        public string? IdempotencyKey { get; set; }

        /// <summary>
        /// Descripción u observaciones breves del movimiento.
        /// </summary>
        public string? Descripcion { get; set; }
    }
}

