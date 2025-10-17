using System;

namespace Bancalite.Application.Cuentas.CuentaResponse
{
    /// <summary>
    /// Respuesta detallada de una cuenta.
    /// </summary>
    public class CuentaDto
    {
        /// <summary>
        /// Identificador de la cuenta.
        /// </summary>
        public Guid CuentaId { get; set; }

        /// <summary>
        /// Número único de la cuenta (UK).
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
        /// Nombre completo del titular de la cuenta.
        /// </summary>
        public string ClienteNombre { get; set; } = string.Empty;

        /// <summary>
        /// Saldo inicial configurado al abrir la cuenta.
        /// </summary>
        public decimal SaldoInicial { get; set; }

        /// <summary>
        /// Saldo actual de la cuenta.
        /// </summary>
        public decimal SaldoActual { get; set; }

        /// <summary>
        /// Estado actual de la cuenta (Activa, Inactiva, Bloqueada).
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de apertura de la cuenta.
        /// </summary>
        public DateTime FechaApertura { get; set; }

        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha de última actualización del registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
