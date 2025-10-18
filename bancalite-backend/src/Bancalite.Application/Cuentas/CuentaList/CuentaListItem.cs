namespace Bancalite.Application.Cuentas.CuentaList
{
    /// <summary>
    /// Elemento de listado de cuentas.
    /// </summary>
    public class CuentaListItem
    {
        /// <summary>
        /// Identificador de la cuenta.
        /// </summary>
        public Guid CuentaId { get; set; }

        /// <summary>
        /// Número único de la cuenta.
        /// </summary>
        public string NumeroCuenta { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del tipo de cuenta.
        /// </summary>
        public Guid TipoCuentaId { get; set; }

        /// <summary>
        /// Nombre del tipo de cuenta.
        /// </summary>
        public string TipoCuentaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del cliente titular.
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Nombre del titular.
        /// </summary>
        public string ClienteNombre { get; set; } = string.Empty;

        /// <summary>
        /// Estado del cliente (true=activo, false=inactivo).
        /// </summary>
        public bool ClienteActivo { get; set; }

        /// <summary>
        /// Saldo actual.
        /// </summary>
        public decimal SaldoActual { get; set; }

        /// <summary>
        /// Estado (Activa, Inactiva, Bloqueada).
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de apertura de la cuenta.
        /// </summary>
        public DateTime FechaApertura { get; set; }
    }
}
