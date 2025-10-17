using System;

namespace Bancalite.Application.Cuentas.CuentaUpdate
{
    /// <summary>
    /// Actualización parcial de cuenta.
    /// </summary>
    public class CuentaPatchRequest
    {
        /// <summary>
        /// Número de cuenta (si se envía, se valida unicidad y longitud).
        /// </summary>
        public string? NumeroCuenta { get; set; }
        /// <summary>
        /// Tipo de cuenta destino (si se envía, debe existir).
        /// </summary>
        public Guid? TipoCuentaId { get; set; }
        /// <summary>
        /// Cliente titular destino (si se envía, debe existir).
        /// </summary>
        public Guid? ClienteId { get; set; }
    }
}
