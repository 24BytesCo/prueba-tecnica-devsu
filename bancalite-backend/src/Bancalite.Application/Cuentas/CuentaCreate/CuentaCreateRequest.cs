namespace Bancalite.Application.Cuentas.CuentaCreate
{
    /// <summary>
    /// Datos para apertura de cuenta.
    /// </summary>
    public class CuentaCreateRequest
    {
        /// <summary>
        /// Número de cuenta (opcional). Si no se envía, el sistema genera uno automáticamente.
        /// </summary>
        public string NumeroCuenta { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del tipo de cuenta.
        /// </summary>
        public Guid TipoCuentaId { get; set; }

        /// <summary>
        /// Identificador del cliente titular.
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Saldo inicial (>= 0).
        /// </summary>
        public decimal SaldoInicial { get; set; }
    }
}
