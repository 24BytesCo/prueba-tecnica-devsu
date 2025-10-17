using System;
using System.Collections.Generic;

namespace Bancalite.Application.Reportes.EstadoCuenta
{
    /// <summary>
    /// DTO del reporte de estado de cuenta (compartido entre JSON y PDF).
    /// </summary>
    public class EstadoCuentaDto
    {
        public Guid? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? NumeroCuenta { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }

        public List<EstadoCuentaItemDto> Movimientos { get; set; } = new();
    }
}
