namespace Bancalite.Domain;

public class Movimiento : BaseEntity
{

    public Guid CuentaId { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Guid TipoId { get; set; }
    public TipoMovimiento Tipo { get; set; } = null!; // Débito/Crédito

    public decimal Monto { get; set; }
    public decimal SaldoPrevio { get; set; }
    public decimal SaldoPosterior { get; set; }

    public string? Descripcion { get; set; }
    public string? CreatedBy { get; set; }
}
