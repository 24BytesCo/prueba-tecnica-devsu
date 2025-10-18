namespace Bancalite.Domain;

public class TipoMovimiento : BaseEntity
{
    public string Codigo { get; set; } = null!; // UK (p.ej., DEB, CRE)
    public string Nombre { get; set; } = null!; // Débito / Crédito
    public bool Activo { get; set; } = true;
    public string? CreatedBy { get; set; }

    // Navegación
    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
