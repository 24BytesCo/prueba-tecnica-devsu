using System;

namespace Bancalite.Application.Cuentas.CuentaEstado
{
    /// <summary>
    /// Cambio de estado de una cuenta.
    /// </summary>
    public class CuentaEstadoPatchRequest
    {
        /// <summary>
        /// Estado destino (Activa, Inactiva, Bloqueada).
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Motivo opcional del cambio.
        /// </summary>
        public string? Motivo { get; set; }
    }
}

