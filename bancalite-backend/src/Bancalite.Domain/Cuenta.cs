namespace Bancalite.Domain;

public class Cuenta : BaseEntity
{
    public string NumeroCuenta { get; set; } = null!; // UK

    public Guid TipoCuentaId { get; set; }
    public TipoCuenta TipoCuenta { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public decimal SaldoInicial { get; set; }
    public decimal SaldoActual { get; set; }
    // Estados permitidos: ACTIVA, INACTIVA, BLOQUEADA
    public EstadoCuenta Estado { get; private set; } = EstadoCuenta.Activa;

    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;

    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    // Reglas de transición de estado (dominio)
    public bool PuedeOperar() => Estado == EstadoCuenta.Activa;
    public void Activar() => Estado = EstadoCuenta.Activa;
    public void Desactivar() => Estado = EstadoCuenta.Inactiva;
    public void Bloquear() => Estado = EstadoCuenta.Bloqueada;
}

