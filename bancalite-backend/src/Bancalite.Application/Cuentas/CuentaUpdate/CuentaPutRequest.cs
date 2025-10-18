namespace Bancalite.Application.Cuentas.CuentaUpdate
{
    /// <summary>
    /// Actualización total de cuenta.
    /// </summary>
    public class CuentaPutRequest
    {
        /// <summary>
        /// Número único de la cuenta.
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
    }
}
