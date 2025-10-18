namespace Bancalite.Domain;

public class TipoCuenta : BaseEntity
{
    public string Codigo { get; set; } = null!; // UK
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
}
